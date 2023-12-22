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
using ControllableDeviceTypes.SonySimpleIPTypes;

namespace ControlAVP.Pages.Devices
{
    public class SonySimpleIPDeviceInfo
    {
        public PowerStatus? PowerStatus { get; set; }
        public InputPort? InputPort { get; set; }
    }

    public class SonySimpleIPModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private readonly string _connectionString;
        private readonly string _deviceId;
        private readonly ServiceClient _serviceClient;
        private readonly SonySimpleIP _device;

        public SonySimpleIPDeviceInfo DeviceInfoCache { get; private set; } = new SonySimpleIPDeviceInfo();

        public SonySimpleIPModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _device = new SonySimpleIP(_serviceClient, _deviceId);
        }

        public void OnGet()
        {
            DeviceInfoCache.PowerStatus = _device.GetPowerStatus();

            if (DeviceInfoCache.PowerStatus != PowerStatus.Off)
            {
                DeviceInfoCache.InputPort = _device.GetInputPort();
            }
            else
            {
                DeviceInfoCache.InputPort = null;
            }
        }

        public IActionResult OnPostTurnOn()
        {
            _device.TurnOn();
            return RedirectToPage();
        }

        public IActionResult OnPostTurnOff()
        {
            _device.TurnOff();
            return RedirectToPage();
        }

        public IActionResult OnPostSetInput(InputPort inputPort)
        {
            _device.SetInputPort(inputPort);
            return RedirectToPage();
        }
    }
}