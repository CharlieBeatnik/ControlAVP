using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AVPCloudToDevice;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Devices;
using ControllableDeviceTypes.ExtronMVX44VGATypes;

namespace ControlAVP.Pages.Devices
{
    public class ExtronMVX44VGAModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;
        private ExtronMVX44VGA _device;

        public class DeviceInfo
        {
            public bool Available;
            public Version Firmware;
            public TieState TieState;
        }
        public DeviceInfo DeviceInfoCache { get; private set; } = new DeviceInfo();

        public ExtronMVX44VGAModel(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _device = new ExtronMVX44VGA(_serviceClient, _deviceId);
        }

        public void OnGet()
        {
            DeviceInfoCache.Available = _device.GetAvailable();
            DeviceInfoCache.Firmware = _device.GetFirmware();
            DeviceInfoCache.TieState = _device.GetTieState();
        }

        public IActionResult OnPostTieInputPortToAllOutputPorts(InputPort inputPort, TieType tieType)
        {
            _device.TieInputPortToAllOutputPorts(inputPort, tieType);
            DeviceInfoCache.TieState = _device.GetTieState();
            return RedirectToPage();
        }
    }
}