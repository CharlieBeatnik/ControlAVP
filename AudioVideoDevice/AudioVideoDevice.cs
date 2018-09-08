using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace ComControl
{
    internal abstract class AudioVideoDevice
    {
        private DataWriter _dataWriter;

        protected SerialDevice _serialPort;
        protected string _partialId;

        public string Id { get; private set; }

        public AudioVideoDevice(string partialId)
        {
            if(string.IsNullOrEmpty(partialId))
            {
                throw new ArgumentException("Must not be Null or Empty", "partialId");
            }

            _partialId = partialId;
        }

        public async Task Initialise()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(aqs);

            foreach(var device in dis)
            {
                if(device.Id.Contains(_partialId))
                {
                    Id = device.Id;
                }
            }

            _serialPort = await SerialDevice.FromIdAsync(Id);
            SetSerialParameters();
            _dataWriter = new DataWriter(_serialPort.OutputStream);
        }

        public async Task WriteString(string str)
        {
            _dataWriter.WriteString(str + "\r");
            
            // Launch an async task to complete the write operation
            await _dataWriter.StoreAsync();
        }

        protected abstract void SetSerialParameters();
    }
}
