using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace AudioVideoDevice
{
    public abstract class AudioVideoSerialDevice
    {
        private DataWriter _dataWriter;
        private DataReader _dataReader;
        private string _partialId;

        protected uint ReadBufferLengthBytes { get; set; } = 1024;
        protected TimeSpan ZeroByteReadTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
        protected TimeSpan WriteTimeout { get; set; } = TimeSpan.FromMilliseconds(250);
        protected TimeSpan ReadTimeout { get; set; } = TimeSpan.FromMilliseconds(250);

        protected abstract uint BaudRate { get; }
        protected abstract SerialStopBitCount StopBits { get; }
        protected abstract ushort DataBits { get; }
        protected abstract SerialParity Parity { get; }

        protected SerialDevice SerialPort { get; set; }

        protected delegate string ProcessString(string input);
        protected ProcessString PreWrite { get; set; } = (x) => { return x; };
        protected ProcessString PostRead { get; set; } = (x) => { return x; };
    

        public string Id { get; private set; }

        public AudioVideoSerialDevice(string partialId)
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
            SerialPort = Task.Run(async () => await SerialDevice.FromIdAsync(Id)).Result;
            Debug.Assert(SerialPort != null);
            SetSerialParameters();

            //Create data writer
            _dataWriter = new DataWriter(SerialPort.OutputStream);

            //Create data reader
            _dataReader = new DataReader(SerialPort.InputStream)
            {
                //Used to allow reading of > 0 bytes within serialPort.ReadTimeout
                InputStreamOptions = InputStreamOptions.Partial
            };
        }

        private void SetSerialParameters()
        {
            SerialPort.WriteTimeout = WriteTimeout;
            SerialPort.ReadTimeout = ReadTimeout;
            SerialPort.BaudRate = BaudRate;
            SerialPort.StopBits = StopBits;
            SerialPort.DataBits = DataBits;
            SerialPort.Parity = Parity;
        }

        public static async Task<DeviceInformationCollection> GetAvailableDevices()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            return await DeviceInformation.FindAllAsync(aqs);
        }

        protected void Write(string write)
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

        protected string Read()
        {
            var cts = new CancellationTokenSource(ZeroByteReadTimeout);
            try
            {
                var loadAsyncTask = _dataReader.LoadAsync(ReadBufferLengthBytes).AsTask(cts.Token);
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
            catch (TaskCanceledException)
            {
                //Exception is thrown in the event of a zero byte read timeout
                return string.Empty;
            }
        }

        protected string WriteWithResponse(string write)
        {
            Write(write);
            return Read();
        }
    }
}
