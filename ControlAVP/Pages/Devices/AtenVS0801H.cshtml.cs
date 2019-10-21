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
using System.Diagnostics;

namespace ControlAVP.Pages.Devices
{
    public class AtenVS0801HModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;

        private const uint _numHdmiSwitches = 2;
        private List<AtenVS0801H> _devices = new List<AtenVS0801H>();

        public class DeviceInfo
        {
            public bool Available;
            public State State;
        }
        public List<DeviceInfo> DeviceInfoCaches { get; private set; } = new List<DeviceInfo>();

        public AtenVS0801HModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);

            for(uint deviceIdx = 0; deviceIdx < _numHdmiSwitches; ++deviceIdx)
            {
                _devices.Add(new AtenVS0801H(_serviceClient, _deviceId, deviceIdx));
                DeviceInfoCaches.Add(new DeviceInfo());
            }
        }

        public void OnGet()
        {
            for (int deviceIndex = 0; deviceIndex < _numHdmiSwitches; ++deviceIndex)
            {
                DeviceInfoCaches[deviceIndex].Available = _devices[deviceIndex].GetAvailable();
                DeviceInfoCaches[deviceIndex].State = _devices[deviceIndex].GetState();
            }
        }

        public IActionResult OnPostSetInputPort(int deviceIndex, InputPort inputPort)
        {
            Debug.Assert(deviceIndex < _numHdmiSwitches);
            if (deviceIndex < _numHdmiSwitches)
            {
                _devices[deviceIndex].SetInputPort(inputPort);
                DeviceInfoCaches[deviceIndex].State = _devices[deviceIndex].GetState();
            }
            return RedirectToPage();
        }
    }
}