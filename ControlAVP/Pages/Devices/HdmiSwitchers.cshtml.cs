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
using ControllableDeviceTypes.AtenVS0801HBTypes;

using System.Diagnostics;

namespace ControlAVP.Pages.Devices
{
    public class AtenVS0801HDeviceInfo
    {
        public bool Available { get; set; }
        public ControllableDeviceTypes.AtenVS0801HTypes.State State { get; set; }
    }

    public class AtenVS0801HBDeviceInfo
    {
        public bool Available { get; set; }
        public ControllableDeviceTypes.AtenVS0801HBTypes.State State { get; set; }
    }

    public class HdmiSwitchersModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private readonly string _connectionString;
        private readonly string _deviceId;
        private readonly ServiceClient _serviceClient;

        private const uint _numAtenVS0801HDevices = 1;
        private readonly List<AtenVS0801H> _atenVS0801HDevices = [];

        private const uint _numAtenVS0801HBDevices = 1;
        private readonly List<AtenVS0801HB> _atenVS0801HBDevices = [];

        public IList<AtenVS0801HDeviceInfo> AtenVS0801HDeviceInfoCaches { get; private set; } = new List<AtenVS0801HDeviceInfo>();
        public IList<AtenVS0801HBDeviceInfo> AtenVS0801HBDeviceInfoCaches { get; private set; } = new List<AtenVS0801HBDeviceInfo>();

        public HdmiSwitchersModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);

            for(uint deviceIndex = 0; deviceIndex < _numAtenVS0801HDevices; ++deviceIndex)
            {
                _atenVS0801HDevices.Add(new AtenVS0801H(_serviceClient, _deviceId, deviceIndex));
                AtenVS0801HDeviceInfoCaches.Add(new AtenVS0801HDeviceInfo());
            }

            for (uint deviceIndex = 0; deviceIndex < _numAtenVS0801HBDevices; ++deviceIndex)
            {
                _atenVS0801HBDevices.Add(new AtenVS0801HB(_serviceClient, _deviceId, deviceIndex));
                AtenVS0801HBDeviceInfoCaches.Add(new AtenVS0801HBDeviceInfo());
            }
        }

        public void OnGet()
        {
            for (int deviceIndex = 0; deviceIndex < _numAtenVS0801HDevices; ++deviceIndex)
            {
                AtenVS0801HDeviceInfoCaches[deviceIndex].Available = _atenVS0801HDevices[deviceIndex].GetAvailable();
                AtenVS0801HDeviceInfoCaches[deviceIndex].State = _atenVS0801HDevices[deviceIndex].GetState();
            }

            for (int deviceIndex = 0; deviceIndex < _numAtenVS0801HBDevices; ++deviceIndex)
            {
                AtenVS0801HBDeviceInfoCaches[deviceIndex].Available = _atenVS0801HBDevices[deviceIndex].GetAvailable();
                AtenVS0801HBDeviceInfoCaches[deviceIndex].State = _atenVS0801HBDevices[deviceIndex].GetState();
            }
        }

        public IActionResult OnPostAtenVS0801HSetInputPort(int deviceIndex, ControllableDeviceTypes.AtenVS0801HTypes.InputPort inputPort)
        {
            Debug.Assert(deviceIndex < _numAtenVS0801HDevices);
            if (deviceIndex < _numAtenVS0801HDevices)
            {
                _atenVS0801HDevices[deviceIndex].SetInputPort(inputPort);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostAtenVS0801HBSetInputPort(int deviceIndex, ControllableDeviceTypes.AtenVS0801HBTypes.InputPort inputPort)
        {
            Debug.Assert(deviceIndex < _numAtenVS0801HDevices);
            if (deviceIndex < _numAtenVS0801HDevices)
            {
                _atenVS0801HBDevices[deviceIndex].SetInputPort(inputPort);
            }
            return RedirectToPage();
        }
    }
}