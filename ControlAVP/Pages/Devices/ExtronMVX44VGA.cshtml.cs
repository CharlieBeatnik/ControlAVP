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
    internal sealed class ExtronMVX44VGADeviceInfo
    {
        public bool Available { get; set; }
        public Version Firmware { get; set; }
        public TieState TieState { get; set; }
    }

    internal sealed class ExtronMVX44VGAModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private readonly string _connectionString;
        private readonly string _deviceId;
        private readonly ServiceClient _serviceClient;
        private readonly ExtronMVX44VGA _device;

        public ExtronMVX44VGADeviceInfo DeviceInfoCache { get; private set; } = new ExtronMVX44VGADeviceInfo();

        public ExtronMVX44VGAModel(IConfiguration configuration, IWebHostEnvironment environment)
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

        public IActionResult OnPostTieInputPortToOutputPort(InputPort inputPort, OutputPort outputPort, TieType tieType)
        {
            _device.TieInputPortToOutputPort(inputPort, outputPort, tieType);
            DeviceInfoCache.TieState = _device.GetTieState();
            return RedirectToPage();
        }
    }
}