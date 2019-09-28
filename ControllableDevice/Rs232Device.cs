using Cyotek.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Timers;
using Windows.Foundation;

namespace ControllableDevice
{
    public class Rs232Device : IDisposable
    {
        private bool _disposed = false;

        private string _partialId;
        private SerialDevice _serialDevice;
        private readonly object _serialDeviceLock = new object();

        private DeviceWatcher _deviceWatcher;

        private DataWriter _dataWriter;
        private DataReader _dataReader;

        private CancellationTokenSource _readCancellationTokenSource;
        private Task _listenTask;
        private CircularBuffer<TimestampedMessage> _messageStore;
        private string _unterminatedMessage = string.Empty;

        private readonly int _messageStoreCapacity = 1024;
        private readonly uint _readBufferLength = 1024;
        private readonly TimeSpan _defaultMonitorDeviceTimerInterval = TimeSpan.FromSeconds(10);

        //Serial device defaults
        private TimeSpan _writeTimeout = TimeSpan.FromMilliseconds(50);
        private TimeSpan _readTimeout = TimeSpan.FromMilliseconds(50);
        private uint _baudRate = 9600;
        private ushort _dataBits = 8;
        private SerialParity _parity = SerialParity.None;
        private SerialStopBitCount _stopBits = SerialStopBitCount.One;

        //Configuration Properties
        public delegate string ProcessString(string input);
        public ProcessString PreWrite { get; set; } = (x) => { return x; };
        public TimeSpan PostWriteWait { get; set; } = TimeSpan.FromMilliseconds(500);
        public TimeSpan MessageLifetime { get; set; } = TimeSpan.FromSeconds(10);
        public string ReceivedMessageTerminator { get; set; } = "\r\n";

        public TimeSpan WriteTimeout
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null)
                    {
                        _writeTimeout = value;
                        _serialDevice.WriteTimeout = _writeTimeout;
                    }
                }
            }
        }

        public TimeSpan ReadTimeout
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null)
                    {
                        _readTimeout = value;
                        _serialDevice.ReadTimeout = _readTimeout;
                    }
                }
            }
        }

        public uint BaudRate
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null)
                    {
                        _baudRate = value;
                        _serialDevice.BaudRate = _baudRate;
                    }
                }
            }
        }

        public ushort DataBits
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null)
                    {
                        _dataBits = value;
                        _serialDevice.DataBits = _dataBits;
                    }
                }
            }
        }

        public SerialParity Parity
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null)
                    {
                        _parity = value;
                        _serialDevice.Parity = _parity;
                    }
                }
            }
        }

        public SerialStopBitCount StopBits
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null)
                    {
                        _stopBits = value;
                        _serialDevice.StopBits = _stopBits;
                    }
                }
            }
        }

        public bool Enabled { get; private set; } = false;

        public Rs232Device(string partialId)
        {
            if (string.IsNullOrEmpty(partialId))
            {
                throw new ArgumentException("Must not be Null or Empty.", "partialId");
            }
            _partialId = partialId;

            InitialiseSerialDevice();

            if (_deviceWatcher == null)
            {
                _deviceWatcher = DeviceInformation.CreateWatcher(SerialDevice.GetDeviceSelector());

                _deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(OnDeviceAdded);
                _deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(OnDeviceRemoved);

                _deviceWatcher.Start();
            }
        }

        private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            if ((deviceInfo.Id.Contains(_partialId)) && !Enabled)
            {
                InitialiseSerialDevice();
            }
        }

        private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
        {
            if (Enabled && (deviceInformationUpdate.Id.Contains(_partialId)))
            {
                DeInitialiseSerialDevice();
            }
        }

        private void InitialiseSerialDevice()
        {
            lock(_serialDeviceLock)
            {
                if (!Enabled)
                {
                    try
                    {
                        Debug.WriteLine("InitialiseSerialDevice: Start");
                        //It's legitimate to try and initialise the device with a partial ID that doesn't exist
                        //For example, a serial device isn't yet connected. Handle this case gracefully by just returning.
                        string id = GetDeviceId(_partialId);
                        if (id == null) return;

                        _serialDevice = Task.Run(async () => await SerialDevice.FromIdAsync(id))?.Result;

                        if (_serialDevice != null)
                        {
                            //Serial port defaults
                            WriteTimeout = _writeTimeout;
                            ReadTimeout = _readTimeout;
                            BaudRate = _baudRate;
                            DataBits = _dataBits;
                            Parity = _parity;
                            StopBits = _stopBits;

                            _serialDevice.BreakSignalState = false;
                            _serialDevice.IsDataTerminalReadyEnabled = true;
                            _serialDevice.IsRequestToSendEnabled = true;

                            //Create data reader/writer
                            _dataWriter = new DataWriter(_serialDevice.OutputStream);
                            _dataReader = new DataReader(_serialDevice.InputStream);

                            _messageStore = new CircularBuffer<TimestampedMessage>(_messageStoreCapacity);

                            //Start async listen task to deal with async reads
                            _readCancellationTokenSource = new CancellationTokenSource();
                            _listenTask = new Task(Listen);
                            _listenTask.Start();

                            Enabled = true;
                            Debug.WriteLine("InitialiseSerialDevice: Finish");
                        }
                    }
                    catch (Exception)
                    {
                        //Many different exception types can be thrown if the serial device is pulled during initialisation
                        return;
                    }
                }
            }
        }

        private void DeInitialiseSerialDevice()
        {
            lock(_serialDeviceLock)
            {
                if (Enabled)
                {
                    Debug.WriteLine("DeInitialiseSerialDevice: Start");
                    Enabled = false;

                    _readCancellationTokenSource?.Cancel();

                    _listenTask?.Wait();
                    _listenTask = null;

                    _readCancellationTokenSource?.Dispose();
                    _readCancellationTokenSource = null;

                    _dataReader?.DetachStream();
                    _dataReader = null;

                    _dataWriter?.DetachStream();
                    _dataReader = null;

                    _serialDevice?.Dispose();
                    _serialDevice = null;
                    Debug.WriteLine("DeInitialiseSerialDevice: Finish");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _deviceWatcher.Stop();
                _deviceWatcher = null;

                DeInitialiseSerialDevice();
            }

            _disposed = true;
        }

        private void Listen()
        {
            try
            {
                if (_serialDevice != null)
                {
                    while (true)
                    {
                        Read(_readCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException || ex is OperationCanceledException)
                {
                    
                }
            }
        }

        private void Read(CancellationToken cancellationToken)
        {
            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Create a task object to wait for data on the serialPort.InputStream
                var loadAsyncTask = _dataReader.LoadAsync(_readBufferLength).AsTask(childCancellationTokenSource.Token);
                loadAsyncTask.Wait();

                var numBytesRead = loadAsyncTask.Result;
                if (numBytesRead > 0)
                {
                    var bytesRead = new byte[numBytesRead];
                    _dataReader.ReadBytes(bytesRead);
                    var readString = System.Text.Encoding.UTF8.GetString(bytesRead);
                    AddToMessageStore(readString);
                }
            }
        }

        private void AddToMessageStore(string readData)
        {
            readData = _unterminatedMessage + readData;
            _unterminatedMessage = string.Empty;

            List<string> messages = readData.Split(ReceivedMessageTerminator, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (messages.Count > 0)
            {
                if (!readData.EndsWith(ReceivedMessageTerminator))
                {
                    _unterminatedMessage = messages.Last();
                    messages.RemoveAt(messages.Count - 1);
                }

                foreach (var message in messages)
                {
                    var timestampedMessage = new TimestampedMessage(message);
                    lock (_messageStore)
                    {
                        _messageStore.Put(timestampedMessage);
                    }
                }
            }
        }

        public void Write(string write)
        {
            if (Enabled)
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
        }

        public string Read(string pattern)
        {
            if (Enabled)
            {
                lock (_messageStore)
                {
                    if (!_messageStore.IsEmpty)
                    {
                        var messageStore = _messageStore.ToList();
                        messageStore.Reverse();

                        foreach (var message in messageStore)
                        {
                            if (message.Age < MessageLifetime)
                            {
                                var match = Regex.Match(message.Message, pattern);
                                if (match.Success)
                                {
                                    return message.Message;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public string WriteWithResponse(string write, string pattern, TimeSpan? postWriteWaitOverride = null)
        {
            if (Enabled)
            {
                Write(write);
                Thread.Sleep(postWriteWaitOverride == null ? PostWriteWait : (TimeSpan)postWriteWaitOverride);
                return Read(pattern);
            }
            else
            {
                return null;
            }
        }

        public List<string> WriteWithResponses(string write, int numResponses, TimeSpan? postWriteWaitOverride = null)
        {
            if (Enabled)
            {
                Write(write);
                Thread.Sleep(postWriteWaitOverride == null ? PostWriteWait : (TimeSpan)postWriteWaitOverride);

                lock (_messageStore)
                {
                    if (!_messageStore.IsEmpty)
                    {
                        List<string> messageStore =
                             (from x in _messageStore
                              where x.Age < MessageLifetime
                              select x.Message).ToList();

                        if (messageStore.Count < numResponses)
                        {
                            throw new ArgumentException($"Unable to return {numResponses} responses, only {messageStore.Count} available.", "numResponses");
                        }
                        int take = Math.Min(numResponses, messageStore.Count);
                        return messageStore.TakeLast(take).ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else return null;
        }

        private static async Task<DeviceInformationCollection> GetAvailableDevices()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            return await DeviceInformation.FindAllAsync(aqs);
        }

        private static string GetDeviceId(string partialId)
        {
            var devices = Task.Run(async () => await GetAvailableDevices()).Result;
            var deviceInformation = devices.FirstOrDefault(s => s.Id.Contains(partialId));

            return deviceInformation?.Id;
        }
    }
}
