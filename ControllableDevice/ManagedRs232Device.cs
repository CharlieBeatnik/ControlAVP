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

namespace ControllableDevice
{
    public class ManagedRs232Device : IDisposable
    {
        private bool _disposed = false;
        private System.Timers.Timer _monitorDeviceTimer;

        private string _partialId;
        private SerialDevice _serialDevice;
        private readonly object _serialDeviceLock = new object();

        private DataWriter _dataWriter;
        private DataReader _dataReader;

        private CancellationTokenSource _readCancellationTokenSource;
        private Task _listenTask;
        private CircularBuffer<TimestampedMessage> _messageStore;
        private string _unterminatedMessage = string.Empty;

        private readonly TimeSpan _defaultWriteTimeout = TimeSpan.FromMilliseconds(50);
        private readonly TimeSpan _defaultReadTimeout = TimeSpan.FromMilliseconds(50);
        private readonly int _messageStoreCapacity = 1024;
        private readonly uint _readBufferLength = 1024;
        private readonly TimeSpan _defaultMonitorDeviceTimerInterval = TimeSpan.FromSeconds(10);

        //Configuration Properties
        public delegate string ProcessString(string input);
        public ProcessString PreWrite { get; set; } = (x) => { return x; };
        public TimeSpan PostWriteWait { get; set; } = TimeSpan.FromMilliseconds(500);
        public TimeSpan MessageLifetime { get; set; } = TimeSpan.FromSeconds(10);
        public string ReceivedMessageTerminator { get; set; } = "\r\n";
        public TimeSpan MonitorDeviceTimerInterval
        {
            get
            {
                return TimeSpan.FromMilliseconds(_monitorDeviceTimer.Interval);
            }
            set
            {
                _monitorDeviceTimer.Interval = value.TotalMilliseconds;
            }
        }

        public TimeSpan WriteTimeout
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null) _serialDevice.WriteTimeout = value;
                }
            }
        }

        public TimeSpan ReadTimeout
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null) _serialDevice.ReadTimeout = value;
                }
            }
        }

        public uint BaudRate
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null) _serialDevice.BaudRate = value;
                }
            }
        }

        public ushort DataBits
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null) _serialDevice.DataBits = value;
                }
            }
        }

        public SerialParity Parity
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null) _serialDevice.Parity = value;
                }
            }
        }

        public SerialStopBitCount StopBits
        {
            set
            {
                lock (_serialDeviceLock)
                {
                    if (_serialDevice != null) _serialDevice.StopBits = value;
                }
            }
        }

        public bool Enabled { get; private set; } = false;

        public ManagedRs232Device(string partialId)
        {
            _partialId = partialId;

            InitialiseSerialDevice();

            _monitorDeviceTimer = new System.Timers.Timer();
            _monitorDeviceTimer.Interval = _defaultMonitorDeviceTimerInterval.TotalMilliseconds;
            _monitorDeviceTimer.Elapsed += MonitorDevice;
            _monitorDeviceTimer.AutoReset = true;
            _monitorDeviceTimer.Enabled = true;
        }

        private void MonitorDevice(Object source, ElapsedEventArgs e)
        {
            if (SerialDeviceConnected(_partialId) && !Enabled)
            {
                InitialiseSerialDevice();
            }
            else if (!SerialDeviceConnected(_partialId) && Enabled)
            {
                DeInitialiseSerialDevice();
            }
        }

        private void InitialiseSerialDevice()
        {
            lock(_serialDeviceLock)
            {
                try
                {
                    //It's legitimate to try and initialise the device with a partial ID that doesn't exist
                    //For example, a serial device isn't yet connected. Handle this case gracefully by just returning.
                    string id = GetDeviceId(_partialId);
                    if (id == null) return;

                    _serialDevice = Task.Run(async () => await SerialDevice.FromIdAsync(id))?.Result;

                    //Serial port defaults
                    WriteTimeout = _defaultWriteTimeout;
                    ReadTimeout = _defaultReadTimeout;
                    BaudRate = 9600;
                    DataBits = 8;
                    Parity = SerialParity.None;
                    StopBits = SerialStopBitCount.One;

                    //Create data reader/writer
                    _dataWriter = new DataWriter(_serialDevice.OutputStream);
                    _dataReader = new DataReader(_serialDevice.InputStream);

                    _messageStore = new CircularBuffer<TimestampedMessage>(_messageStoreCapacity);

                    //Start async listen task to deal with async reads
                    _readCancellationTokenSource = new CancellationTokenSource();
                    _listenTask = new Task(Listen);
                    _listenTask.Start();

                    Enabled = true;
                }
                catch(Exception)
                {
                    //Many different exception types can be thrown if the serial device is pulled during initialisation
                    return;
                }
            }
        }

        private void DeInitialiseSerialDevice()
        {
            lock(_serialDeviceLock)
            {
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
                _monitorDeviceTimer.Enabled = false;
                _monitorDeviceTimer = null;

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

        private static bool SerialDeviceConnected(string partialId)
        {
            return GetDeviceId(partialId) != null;
        }
    }
}
