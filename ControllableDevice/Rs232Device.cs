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

        private readonly TimeSpan _defaultWriteTimeout = TimeSpan.FromMilliseconds(150);
        public TimeSpan WriteTimeout
        {
            get { return _serialPort.WriteTimeout; }
            set { _serialPort.WriteTimeout = value; }
        }

        private readonly TimeSpan _defaultReadTimeout = TimeSpan.FromMilliseconds(150);
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

        public uint MaximumBytesPerRead { get; set; } = 1024;

        public TimeSpan ZeroByteReadTimeout { get; set; } = TimeSpan.FromMilliseconds(175);

        public bool UseFastReadBeforeEveryWrite { get; set; } = false;
        public TimeSpan FastReadReadTimeout { get; set; } = TimeSpan.FromMilliseconds(10);
        public TimeSpan FastReadZeroByteReadTimeout { get; set; } = TimeSpan.FromMilliseconds(15);

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

            //Serial port defaults
            WriteTimeout = _defaultWriteTimeout;
            ReadTimeout = _defaultReadTimeout;
            BaudRate = 9600;
            DataBits = 8;
            Parity = SerialParity.None;
            StopBits = SerialStopBitCount.One;

            //Create data writer
            _dataWriter = new DataWriter(_serialPort.OutputStream);

            //Create data reader
            _dataReader = new DataReader(_serialPort.InputStream)
            {
                //Used to allow reading of > 0 bytes within serialPort.ReadTimeout
                InputStreamOptions = InputStreamOptions.Partial
            };
        }

        public static async Task<DeviceInformationCollection> GetAvailableDevices()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            return await DeviceInformation.FindAllAsync(aqs);
        }

        public void Write(string write)
        {
            if (UseFastReadBeforeEveryWrite)
            {
                FastRead();
            }

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
            var cts = new CancellationTokenSource(ZeroByteReadTimeout);
            try
            {
                var loadAsyncTask = _dataReader.LoadAsync(MaximumBytesPerRead).AsTask(cts.Token);
                loadAsyncTask.Wait();

                var bytesRead = loadAsyncTask.Result;
                Debug.Assert(bytesRead > 0);
                Debug.Assert(loadAsyncTask.IsCompleted == true);
                Debug.Assert(loadAsyncTask.IsCompletedSuccessfully == true);
                Debug.Assert(loadAsyncTask.Status == TaskStatus.RanToCompletion);

                var readString = _dataReader.ReadString(bytesRead);
                var processedReadString = PostRead(readString);

                return processedReadString;
            }
            catch (Exception e)
            {
                if (e.InnerException is TaskCanceledException)
                {
                    //Exception is thrown in the event of a zero byte read timeout
                    return string.Empty;
                }
                else throw e;
            }
        }

        public string FastRead()
        {
            var previousZeroByteReadTimeout = ZeroByteReadTimeout;
            var previousReadTimeout = ReadTimeout;

            ZeroByteReadTimeout = FastReadZeroByteReadTimeout;
            ReadTimeout = FastReadReadTimeout;

            var result = Read();

            ZeroByteReadTimeout = previousZeroByteReadTimeout;
            ReadTimeout = previousReadTimeout;

            return result;
        }

        public string WriteWithResponse(string write)
        {
            Write(write);
            return Read();
        }
    }
}
