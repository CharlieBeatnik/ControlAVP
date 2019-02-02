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
    public class Rs232Device
    {
        private DataWriter _dataWriter;
        private DataReader _dataReader;
        private string _partialId;
        private SerialDevice _serialPort;
        private CancellationTokenSource _readCancellationTokenSource;
        private CircularBuffer<string> _messageStore;
        static string _unterminatedMessage = string.Empty;

        private readonly TimeSpan _defaultWriteTimeout = TimeSpan.FromMilliseconds(50);
        private readonly TimeSpan _defaultReadTimeout = TimeSpan.FromMilliseconds(50);
        private readonly int _defaultMessageStoreCapacity = 30;

        public delegate string ProcessString(string input);
        public ProcessString PreWrite { get; set; } = (x) => { return x; };

        public string Id { get; private set; }
        public uint MaximumBytesPerRead { get; set; } = 1024;
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

        public TimeSpan PostWriteWait { get; set; } = TimeSpan.FromMilliseconds(750);

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

        public Rs232Device(string partialId, int readStoreCapacity = 0)
        {
            if (string.IsNullOrEmpty(partialId))
            {
                throw new ArgumentException("Must not be Null or Empty", "partialId");
            }
            _partialId = partialId;

            if (readStoreCapacity == 0)
            {
                readStoreCapacity = _defaultMessageStoreCapacity;
            }
            _messageStore = new CircularBuffer<string>(readStoreCapacity);

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

            //Start async listen task to deal with async reads
            _readCancellationTokenSource = new CancellationTokenSource();
            Listen();
        }

        private async void Listen()
        {
            try
            {
                if (_serialPort != null)
                {
                    _dataReader = new DataReader(_serialPort.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(_readCancellationTokenSource.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                CloseDevice();
            }
            catch (Exception)
            {
            }
            finally
            {
                // Cleanup once complete
                if (_dataReader != null)
                {
                    _dataReader.DetachStream();
                    _dataReader = null;
                }
            }
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = MaximumBytesPerRead;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Create a task object to wait for data on the serialPort.InputStream
                loadAsyncTask = _dataReader.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);

                // Launch the task and wait
                UInt32 bytesRead = await loadAsyncTask;
                if (bytesRead > 0)
                {
                    string readData = _dataReader.ReadString(bytesRead);
                    AddToMessageStore(readData);
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
                    lock (_messageStore)
                    {
                        _messageStore.Put(message);
                    }
                }
            }
        }

        private void CloseDevice()
        {
            if (_serialPort != null)
            {
                _serialPort.Dispose();
            }
            _serialPort = null;

            lock (_messageStore)
            {
                _messageStore.Clear();
            }
        }

        public static async Task<DeviceInformationCollection> GetAvailableDevices()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            return await DeviceInformation.FindAllAsync(aqs);
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

        public string WriteWithResponse(string write)
        {
            Write(write);
            Thread.Sleep(PostWriteWait);

            lock (_messageStore)
            {
                if (!_messageStore.IsEmpty)
                {
                    string read = _messageStore.PeekLast();
                    return read;
                }
                else
                {
                    return null;
                }
            }
        }

        public string WriteWithResponse(string write, string pattern)
        {
            Write(write);
            Thread.Sleep(PostWriteWait);

            lock (_messageStore)
            {
                if (!_messageStore.IsEmpty)
                {
                    var messageStore = _messageStore.ToList();
                    messageStore.Reverse();

                    foreach (var message in messageStore)
                    {
                        var match = Regex.Match(message, pattern);
                        if (match.Success)
                        {
                            return message;
                        }
                    }
                }
            }

            return null;
        }

        public List<string> WriteWithResponses(string write, int numResponses)
        {
            Write(write);
            Thread.Sleep(PostWriteWait);

            lock (_messageStore)
            {
                if (!_messageStore.IsEmpty)
                {
                    var messageStore = _messageStore.ToList();
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
