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
using ControllableDeviceTypes.SonyKDL60W855Types;

namespace ControlAVP.Pages.Devices
{
    public class SonyKDL60W855DeviceInfo
    {
        public PowerStatus? PowerStatus { get; set; }
        public InputPort? InputPort { get; set; }
    }

    public class SonyKDL60W855Model : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;
        private SonyKDL60W855 _device;

        public SonyKDL60W855DeviceInfo DeviceInfoCache { get; private set; } = new SonyKDL60W855DeviceInfo();

        public SonyKDL60W855Model(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _device = new SonyKDL60W855(_serviceClient, _deviceId);
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
            DeviceInfoCache.PowerStatus = _device.GetPowerStatus();
            return RedirectToPage();
        }

        public IActionResult OnPostTurnOff()
        {
            _device.TurnOff();
            DeviceInfoCache.PowerStatus = _device.GetPowerStatus();
            return RedirectToPage();
        }

        public IActionResult OnPostSetInput(InputPort inputPort)
        {
            _device.SetInputPort(inputPort);
            DeviceInfoCache.InputPort = _device.GetInputPort();
            return RedirectToPage();
        }
    }
}