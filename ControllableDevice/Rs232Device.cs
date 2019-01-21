using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace ControllableDevice
{
    public class Rs232Device
    {
        private DataWriter _dataWriter;
        private DataReader _dataReader;
        private string _partialId;

        private SerialDevice _serialPort;
        private uint _batchReadSize = 4;

        private readonly TimeSpan _defaultReadTimeout = TimeSpan.FromMilliseconds(10);
        private readonly TimeSpan _defaultZeroByteReadTimeout = TimeSpan.FromMilliseconds(15);

        public TimeSpan ZeroByteReadTimeout{ get; set; }

        public TimeSpan ReadTimeout
        {
            get { return _serialPort.ReadTimeout; }
            set { _serialPort.ReadTimeout = value; }
        }

        public uint BaudRate
        {
            get { return _serialPort.BaudRate; }
            set { _serialPort.BaudRate = value; }
        }

        public ushort DataBits
        {
            get { return _serialPort.DataBits; }
            set { _serialPort.DataBits = value; }
        }

        public SerialParity Parity
        {
            get { return _serialPort.Parity; }
            set { _serialPort.Parity = value; }
        }

        public SerialStopBitCount StopBits
        {
            get { return _serialPort.StopBits; }
            set { _serialPort.StopBits = value; }
        }

        public delegate string ProcessString(string input);
        public ProcessString PreWrite { get; set; } = (x) => { return x; };
        public ProcessString PostRead { get; set; } = (x) => { return x; };
    

        public string Id { get; private set; }

        public Rs232Device(string partialId)
        {
            if (string.IsNullOrEmpty(partialId))
            {
                throw new ArgumentException("Must not be Null or Empty", "partialId");
            }

            _partialId = partialId;

            //Find device ID
            var devices = Task.Run(async () => await GetAvailableDevices()).Result;
            Id = devices.Single(s => s.Id.Contains(_partialId)).Id;

            //Create serial port
            _serialPort = Task.Run(async () => await SerialDevice.FromIdAsync(Id)).Result;
            Debug.Assert(_serialPort != null);

            SetSerialDefaultParameters();
            ZeroByteReadTimeout = _defaultZeroByteReadTimeout;

            //Create data writer
            _dataWriter = new DataWriter(_serialPort.OutputStream);

            //Create data reader
            _dataReader = new DataReader(_serialPort.InputStream)
            {
                //Used to allow reading of > 0 bytes within _serialPort.ReadTimeout
                InputStreamOptions = InputStreamOptions.Partial
            };
        }

        private void SetSerialDefaultParameters()
        {
            //WriteTimeout = _defaultWriteTimeout;
            ReadTimeout = _defaultReadTimeout;

            BaudRate = 9600;
            DataBits = 8;
            Parity = SerialParity.None;
            StopBits = SerialStopBitCount.One;
        }

        public static async Task<DeviceInformationCollection> GetAvailableDevices()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            return await DeviceInformation.FindAllAsync(aqs);
        }

        public void Write(string write)
        {
            var processedWriteString = PreWrite(write);
            _dataWriter.WriteString(processedWriteString);

            var storeAsyncTask = _dataWriter.StoreAsync().AsTask();
            storeAsyncTask.Wait();

            var bytesWritten = storeAsyncTask.Result;
            Debug.Assert(bytesWritten == processedWriteString.Length);
            Debug.Assert(storeAsyncTask.IsCompleted == true);
            Debug.Assert(storeAsyncTask.IsCompletedSuccessfully == true);
            Debug.Assert(storeAsyncTask.Status == TaskStatus.RanToCompletion);
        }
        public string Read()
        {
            string readString = string.Empty;

            try
            {
                while(true)
                {
                    var cts = new CancellationTokenSource(ZeroByteReadTimeout);

                    var loadAsyncTask = _dataReader.LoadAsync(_batchReadSize).AsTask(cts.Token);
                    loadAsyncTask.Wait();

                    var bytesRead = loadAsyncTask.Result;
                    Debug.Assert(bytesRead > 0);
                    Debug.Assert(loadAsyncTask.IsCompleted == true);
                    Debug.Assert(loadAsyncTask.IsCompletedSuccessfully == true);
                    Debug.Assert(loadAsyncTask.Status == TaskStatus.RanToCompletion);

                    readString += _dataReader.ReadString(bytesRead);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException is TaskCanceledException)
                {
                    //Exception is thrown in the event of a zero byte read timeout
                    var processedReadString = PostRead(readString);
                    return processedReadString;
                }
                else throw e;
            }
        }

        public string ClearWriteWithResponse(string write)
        {
            // Perform a read to clear any characters already waiting to be read
            Read();

            Write(write);
            return Read();
        }
    }
}
