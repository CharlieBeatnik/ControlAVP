using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace ComControl
{
    internal abstract class AudioVideoDevice
    {
        private DataWriter _dataWriter;
        private DataReader _dataReader;
        private string _partialId;

        protected uint ReadBufferLengthBytes { get; set; } = 1024;
        protected TimeSpan ZeroByteReadTimeout { get; set; } = TimeSpan.FromMilliseconds(750);
        protected TimeSpan WriteTimeout { get; set; } = TimeSpan.FromMilliseconds(250);
        protected TimeSpan ReadTimeout { get; set; } = TimeSpan.FromMilliseconds(250);
        protected uint BaudRate { get; set; } = 19200;
        protected SerialStopBitCount StopBits { get; set; } = SerialStopBitCount.One;
        protected ushort DataBits { get; set; } = 8;
        protected SerialParity Parity { get; set; } = SerialParity.None;

        protected SerialDevice SerialPort { get; set; }


        protected abstract string _sendLineEnding { get; }

        public string Id { get; private set; }

        public AudioVideoDevice(string partialId)
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

        protected async Task WriteAsync(string write)
        {
            _dataWriter.WriteString(write + _sendLineEnding);
            await _dataWriter.StoreAsync();
        }

        protected void Write(string write)
        {
            Task.Run(async () => await WriteAsync(write));
        }

        protected async Task<string> ReadAsync()
        {
            var cts = new CancellationTokenSource(ZeroByteReadTimeout);

            try
            {
                var bytesRead = await _dataReader.LoadAsync(ReadBufferLengthBytes).AsTask(cts.Token);
                Debug.Assert(bytesRead > 0);
                return _dataReader.ReadString(bytesRead);
            }
            catch (TaskCanceledException)
            {
                //Exception is thrown in the event of a zero byte read timeout
                return string.Empty;
            }
        }

        protected string Read()
        {
            return Task.Run(async () => await ReadAsync()).Result;
        }

        protected async Task<string> WriteWithResponseAsync(string write)
        {
            await WriteAsync(write);
            return await ReadAsync();
        }

        protected string WriteWithResponse(string write)
        {
            return Task.Run(async () => await WriteWithResponseAsync(write)).Result;
        }
    }
}
