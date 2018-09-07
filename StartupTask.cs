using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using System.IO.Ports;
using Windows.Devices.Enumeration;
using WindowsSerialDevice = Windows.Devices.SerialCommunication.SerialDevice;
using Windows.Devices.SerialCommunication;
using System.Threading;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Ports;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409



namespace ComControl
{
    public sealed class StartupTask : IBackgroundTask
    {
        private static BackgroundTaskDeferral _Deferral = null;

        private SerialDevice serialPort = null;
        private CancellationTokenSource ReadCancellationTokenSource;
        private DataWriter dataWriteObject = null;
        private DataReader dataReaderObject = null;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            SerialHack2();

            _Deferral = taskInstance.GetDeferral();

            var webserver = new WebServer();

            await Windows.System.Threading.ThreadPool.RunAsync(workItem =>
            {

                webserver.Start();
            });
        }

        //private async void SerialHack()
        //{
        //    Console.WriteLine("SerialHack");

        //    //Find
        //    string aqs = SerialDevice.GetDeviceSelector();
        //    var dis = await DeviceInformation.FindAllAsync(aqs);

        //    //Connect
        //    DeviceInformation entry = dis[1];

        //    serialPort = await SerialDevice.FromIdAsync(entry.Id);
        //    if (serialPort == null) return;

        //    // Configure serial settings
        //    serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
        //    serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
        //    serialPort.BaudRate = 19200;
        //    serialPort.Parity = SerialParity.None;
        //    serialPort.StopBits = SerialStopBitCount.One;
        //    serialPort.DataBits = 8;
        //    serialPort.Handshake = SerialHandshake.None;

        //    //var send = new byte[] { 0x73, 0x77, 0x69, 0x30, 0x32 };

        //    var write = new DataWriter(serialPort.OutputStream);
        //    //write.WriteBytes(send);
        //    write.ByteOrder = ByteOrder.BigEndian;
        //    write.WriteString("swi04");
        //    var bytesWritten = await write.StoreAsync();

        //    write.WriteString("swi05");
        //    bytesWritten = await write.StoreAsync();

        //    write.ByteOrder = ByteOrder.LittleEndian;
        //    write.WriteString("swi06");
        //    bytesWritten = await write.StoreAsync();

        //    write.WriteString("swi07");
        //    bytesWritten = await write.StoreAsync();

        //    // Create cancellation token object to close I/O operations when closing the device
        //    //ReadCancellationTokenSource = new CancellationTokenSource();

        //    //Listen();

        //    //dataWriteObject = new DataWriter(serialPort.OutputStream);
        //    //await WriteAsync("swi02");
        //}

        private void SerialHack2()
        {
            var names = ComPort.GetPortNames();

            var port = new ComPort(names[1]);
            port.Open();

        }

        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
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
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        /// <summary>
        /// WriteAsync: Task that asynchronously writes data from the input text box 'sendText' to the OutputStream 
        /// </summary>
        /// <returns></returns>
        private async Task WriteAsync(string toSend)
        {
            Task<UInt32> storeAsyncTask;

            if (toSend.Length != 0)
            {
                // Load the text from the sendText input text box to the dataWriter object
                dataWriteObject.WriteString(toSend);

                // Launch an async task to complete the write operation
                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                if (bytesWritten > 0)
                {
                }
            }
            else
            {
            }
        }

        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Create a task object to wait for data on the serialPort.InputStream
                loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);

                // Launch the task and wait
                UInt32 bytesRead = await loadAsyncTask;
                if (bytesRead > 0)
                {
                    Console.WriteLine(dataReaderObject.ReadString(bytesRead));
                }
            }
        }

        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
        private void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }

        /// <summary>
        /// CloseDevice:
        /// - Disposes SerialDevice object
        /// - Clears the enumerated device Id list
        /// </summary>
        private void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;
        }

    }
}
