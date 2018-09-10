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

        private const uint _readBufferLengthBytes = 1024;
        private readonly TimeSpan _zeroByteReadTimeout = TimeSpan.FromMilliseconds(2000);

        protected SerialDevice _serialPort;
        protected string _partialId;

        protected abstract string _sendLineEnding { get; }

        public string Id { get; private set; }

        public AudioVideoDevice(string partialId)
        {
            if(string.IsNullOrEmpty(partialId))
            {
                throw new ArgumentException("Must not be Null or Empty", "partialId");
            }

            _partialId = partialId;

            //Find device ID
            var devices = Task.Run(async () => await GetAvailableDevices()).Result;
            Id = devices.Single(s => s.Id.Contains(_partialId)).Id;

            //Create serial port
            _serialPort = Task.Run(async () => await SerialDevice.FromIdAsync(Id)).Result;

            //Create data writer
            _dataWriter = new DataWriter(_serialPort.OutputStream);

            //Create data reader
            _dataReader = new DataReader(_serialPort.InputStream)
            {
                //Used to allow reading of > 0 bytes within serialPort.ReadTimeout
                InputStreamOptions = InputStreamOptions.Partial 
            };

            SetSerialParameters();
        }

        public static async Task<DeviceInformationCollection> GetAvailableDevices()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            return await DeviceInformation.FindAllAsync(aqs);
        }

        public async Task WriteAsync(string write)
        {
            _dataWriter.WriteString(write + _sendLineEnding);
            await _dataWriter.StoreAsync();
        }

        public void Write(string write)
        {
            Task.Run(async () => await WriteAsync(write));
        }

        public async Task<string> ReadAsync()
        {
            var cts = new CancellationTokenSource(_zeroByteReadTimeout);

            try
            {
                var bytesRead = await _dataReader.LoadAsync(_readBufferLengthBytes).AsTask(cts.Token);
                Debug.Assert(bytesRead > 0);
                return _dataReader.ReadString(bytesRead);
            }
            catch (TaskCanceledException)
            {
                //Exception is thrown in the event of a zero byte read timeout
                return string.Empty;
            }
        }

        public string Read()
        {
            return Task.Run(async () => await ReadAsync()).Result;
        }


        public async Task<string> WriteWithResponseAsync(string write)
        {
            await WriteAsync(write);
            return await ReadAsync();
        }

        public string WriteWithResponse(string write)
        {
            return Task.Run(async () => await WriteWithResponseAsync(write)).Result;
        }

        protected abstract void SetSerialParameters();
    }
}
