using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using EventHub;
using System.Threading;
using CommandProcessor;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace ControlAVP
{
    public class TailCommandProcessorModel
    {
        public IList<CommandResult> CommandResults { get; } = new List<CommandResult>();
        public bool Completed { get; set; }
        public string WebRootPath { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class TailCommandProcessor : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private readonly string _eventHubConnectionString;
        private readonly string _eventHubName;

        private readonly SmartEventHubConsumer _smartEventHubConsumer;
        private CancellationTokenSource _cts;

        private bool disposed;

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
            Response.Headers.Append("Content-Type", "text/event-stream");

            _smartEventHubConsumer.RegisterEventQueue(id);

            var model = new TailCommandProcessorModel();
            model.WebRootPath = _environment.WebRootPath;

            //Send an empty table back for initial display
            await RenderCommandProcessorTableAndReturnResponse(this, model, Response).ConfigureAwait(false);

            //5 minute timeout ensures that if an incorrect or out of date GUID is used that this Get function doesn't run forever
            foreach (var commandResult in _smartEventHubConsumer.GetEvents<CommandResult>(id, TimeSpan.FromMinutes(5)))
            {
                model.CommandResults.Add(commandResult);
                await RenderCommandProcessorTableAndReturnResponse(this, model, Response).ConfigureAwait(false);
            }

            model.Completed = true;
            await RenderCommandProcessorTableAndReturnResponse(this, model, Response).ConfigureAwait(false);

            _smartEventHubConsumer.DeregisterEventQueue(id);
        }

        private static async Task RenderCommandProcessorTableAndReturnResponse(TailCommandProcessor controller, TailCommandProcessorModel model, HttpResponse response)
        {
            var partialViewHtml = await controller.RenderViewAsync("_CommandProcessorTable", model, true).ConfigureAwait(false);

            string json = JsonConvert.SerializeObject(partialViewHtml);
            byte[] messageBytes = ASCIIEncoding.ASCII.GetBytes($"data:{json}\n\n");
            
            await response.Body.WriteAsync(messageBytes.AsMemory(0, messageBytes.Length)).ConfigureAwait(false);
            await response.Body.FlushAsync().ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
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
                disposed = true;
            }
            base.Dispose(disposing);
        }

    }
}
