using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using AVPCloudToDevice;
using ControllableDeviceTypes.AtenVS0801HTypes;

namespace ControlAVP.Pages.Devices
{
    public class AtenVS0801HModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;

        private const uint _numHdmiSwitches = 2;
        private List<AtenVS0801H> _devices = new List<AtenVS0801H>();

        public List<State> States { get; private set; } = new List<State>();

        public AtenVS0801HModel(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);

            for(uint deviceIdx = 0; deviceIdx < _numHdmiSwitches; ++deviceIdx)
            {
                _devices.Add(new AtenVS0801H(_serviceClient, _deviceId, deviceIdx));
            }
        }

        public void OnGet()
        {
            var state = _devices[0].GetState();

            if (state != null)
            {
                States.Add(state);
            }
        }
    }
}