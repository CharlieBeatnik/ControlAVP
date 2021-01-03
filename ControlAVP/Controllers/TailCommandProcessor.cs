using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using EventHub;
using System.Threading;
using CommandProcessor;

namespace ControlAVP
{
    [Route("api/[controller]")]
    [ApiController]
    public class TailCommandProcessor : ControllerBase, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private string _eventHubConnectionString;
        private string _eventHubName;

        private SmartEventHubConsumer _smartEventHubConsumer;
        private CancellationTokenSource _cts;

        public TailCommandProcessor(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _eventHubConnectionString = _configuration.GetValue<string>("ControlAVPEventHubConnectionString");
            _eventHubName = _configuration.GetValue<string>("ControlAVPEventHubName");

            _smartEventHubConsumer = new SmartEventHubConsumer(_eventHubConnectionString, _eventHubName);

            _cts = new CancellationTokenSource();
            _ = _smartEventHubConsumer.ReceiveEventsFromDeviceAsync(_cts.Token);
        }

        [HttpGet]
        public async Task Get(Guid id)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");

            _smartEventHubConsumer.RegisterEventQueue(id);

            foreach(var result in _smartEventHubConsumer.GetEvents(id, TimeSpan.FromMinutes(5)))
            {
                byte[] messageBytes = ASCIIEncoding.ASCII.GetBytes($"data:{result}\n\n");
                await Response.Body.WriteAsync(messageBytes.AsMemory(0, messageBytes.Length)).ConfigureAwait(false);
                await Response.Body.FlushAsync().ConfigureAwait(false);
            }

            _smartEventHubConsumer.DeregisterEventQueue(id);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts.Token.WaitHandle.WaitOne();

                    _cts.Dispose();
                    _cts = null;
                }
            }
        }
    }
}
