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

namespace ControllableDevice
{
    public class TimestampedMessage
    {
        public TimestampedMessage(string message)
        {
            Recieved = DateTime.Now;
            Message = message;
        }
        public DateTime Recieved { private set; get; }
        public string Message { private set; get; }

        public TimeSpan Age { get { return DateTime.Now - Recieved; } }
    }

    public class Rs232Device : IDisposable
    {
        private bool _disposed = false;

        private string _partialId;
        private SerialDevice _serialPort;

        private CancellationTokenSource _readCancellationTokenSource;
        private DataWriter _dataWriter;
        private DataReader _dataReader;

        private Task _listenTask;
        private CircularBuffer<TimestampedMessage> _messageStore;
        private string _unterminatedMessage = string.Empty;

        private readonly TimeSpan _defaultWriteTimeout = TimeSpan.FromMilliseconds(50);
        private readonly TimeSpan _defaultReadTimeout = TimeSpan.FromMilliseconds(50);
        private readonly uint _defaultMessageStoreCapacity = 1024;

        public delegate string ProcessString(string input);
        public ProcessString PreWrite { get; set; } = (x) => { return x; };

        public string Id { get; private set; }
        public uint ReadBufferLength { get; set; } = 1024;
        public string MessageTerminator { get; set; } = "\r\n";

        public TimeSpan WriteTimeout
        {
            get { return _serialPort.WriteTimeout; }
            set { _serialPort.WriteTimeout = value; }
        }

        public TimeSpan ReadTimeout
        {
            get { return _serialPort.ReadTimeout; }
            set { _serialPort.ReadTimeout = value; }
        }

        public TimeSpan PostWriteWait { get; set; } = TimeSpan.FromMilliseconds(500);
        public TimeSpan MessageLifetime { get; set; } = TimeSpan.FromSeconds(10);

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

        public static async Task<DeviceInformationCollection> GetAvailableDevices()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            return await DeviceInformation.FindAllAsync(aqs);
        }

        public Rs232Device(string partialId, uint messageStoreCapacity = 0)
        {
            if (string.IsNullOrEmpty(partialId))
            {
                throw new ArgumentException("Must not be Null or Empty.", "partialId");
            }
            _partialId = partialId;

            if (messageStoreCapacity == 0)
            {
                messageStoreCapacity = _defaultMessageStoreCapacity;
            }
            _messageStore = new CircularBuffer<TimestampedMessage>((int)messageStoreCapacity);

            //Find device ID
            var devices = Task.Run(async () => await GetAvailableDevices()).Result;
            Id = devices.Single(s => s.Id.Contains(_partialId)).Id;

            //Create serial port
            _serialPort = Task.Run(async () => await SerialDevice.FromIdAsync(Id)).Result;
            if(_serialPort == null)
            {
                throw new Exception("Unable to create SerialDevice, is it already is use?");
            }

            //Serial port defaults
            WriteTimeout = _defaultWriteTimeout;
            ReadTimeout = _defaultReadTimeout;
            BaudRate = 9600;
            DataBits = 8;
            Parity = SerialParity.None;
            StopBits = SerialStopBitCount.One;

            //Create data reader/writer
            _dataWriter = new DataWriter(_serialPort.OutputStream);
            _dataReader = new DataReader(_serialPort.InputStream);

            //Start async listen task to deal with async reads
            _readCancellationTokenSource = new CancellationTokenSource();
            _listenTask = new Task(Listen);
            _listenTask.Start();
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
                _readCancellationTokenSource?.Cancel();

                _listenTask?.Wait();
                _listenTask = null;

                _readCancellationTokenSource?.Dispose();
                _readCancellationTokenSource = null;

                CloseDevice();
            }

            _disposed = true;
        }

        private void CloseDevice()
        {
            _dataReader?.DetachStream();
            _dataReader = null;

            _dataWriter?.DetachStream();
            _dataReader = null;

            _serialPort?.Dispose();
            _serialPort = null;

            _messageStore = null;
        }

        private void Listen()
        {
            try
            {
                if (_serialPort != null)
                {
                    while (true)
                    {
                        Debug.WriteLine("Listen Loop");
                        Read(_readCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException || ex is OperationCanceledException)
                {
                    CloseDevice();
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
                var loadAsyncTask = _dataReader.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);
                Debug.WriteLine("loadAsyncTask.Wait()");
                loadAsyncTask.Wait();

                var numBytesRead = loadAsyncTask.Result;
                if (numBytesRead > 0)
                {
                    var bytesRead = new byte[numBytesRead];
                    _dataReader.ReadBytes(bytesRead);
                    var readString = System.Text.Encoding.UTF8.GetString(bytesRead);
                    Debug.WriteLine($"AddToMessageStore: {readString}");
                    AddToMessageStore(readString);
                }
            }
        }

        private void AddToMessageStore(string readData)
        {
            readData = _unterminatedMessage + readData;
            _unterminatedMessage = string.Empty;

            List<string> messages = readData.Split(MessageTerminator, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (messages.Count > 0)
            {
                if (!readData.EndsWith(MessageTerminator))
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
            var processedWriteString = PreWrite(write);

            lock (_dataReader)
            {
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

            return null;
        }

        public string WriteWithResponse(string write, string pattern, TimeSpan? postWriteWaitOverride = null)
        {
            Debug.WriteLine($"WriteWithResponse: {write}");
            Write(write);
            Thread.Sleep(postWriteWaitOverride == null ? PostWriteWait : (TimeSpan)postWriteWaitOverride);
            return Read(pattern);
        }

        public List<string> WriteWithResponses(string write, int numResponses, TimeSpan? postWriteWaitOverride = null)
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
    }
}
