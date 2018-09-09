using System;
using System.Collections.Generic;
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

        private const uint _readBufferLength = 1024;

        protected SerialDevice _serialPort;
        protected string _partialId;

        protected abstract string _sendLineEnding
        {
            get;
        }

        public string Id { get; private set; }

        public bool IsOpen { get; private set; }

        public AudioVideoDevice(string partialId)
        {
            if(string.IsNullOrEmpty(partialId))
            {
                throw new ArgumentException("Must not be Null or Empty", "partialId");
            }

            _partialId = partialId;
            IsOpen = false;
        }

        private async Task<DeviceInformationCollection> GetAvailableDevices()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            return await DeviceInformation.FindAllAsync(aqs);
        }

        private void ValidateDeviceIsOpen()
        {
            if(!IsOpen)
            {
                throw new Exception("Device is not open, OpenDevice() must be called first.");
            }
        }

        public async Task Open()
        {
            //Get list of devices
            var devices = await GetAvailableDevices();
            Id = devices.Single(s => s.Id.Contains(_partialId)).Id;

            //Create serial port
            _serialPort = await SerialDevice.FromIdAsync(Id);
            SetSerialParameters();

            //Create data writer
            _dataWriter = new DataWriter(_serialPort.OutputStream);

            //Create data reader
            _dataReader = new DataReader(_serialPort.InputStream);
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;

            IsOpen = true;
        }

        public async Task WriteString(string str)
        {
            ValidateDeviceIsOpen();

            _dataWriter.WriteString(str + _sendLineEnding);
            await _dataWriter.StoreAsync();
        }

        public async Task<string> ReadString()
        {
            ValidateDeviceIsOpen();

            var bytesRead = await _dataReader.LoadAsync(_readBufferLength);
            if (bytesRead > 0)
            {
                return _dataReader.ReadString(bytesRead);
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<string> WriteStringReadString(string str)
        {
            ValidateDeviceIsOpen();

            await WriteString(str);
            return await ReadString();
        }

        protected abstract void SetSerialParameters();
    }
}
