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
using ControllableDeviceTypes.ExtronDSC301HDTypes;

namespace ControlAVP.Pages.Devices
{
    public class ExtronDSC301HDModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;
        private ExtronDSC301HD _device;

        public class DeviceInfo
        {
            public bool Available;
            public Version Firmware;
            public InputPort? InputPort;
            public float? Temperature;
        }
        public DeviceInfo DeviceInfoCache { get; private set; } = new DeviceInfo();

        public ExtronDSC301HDModel(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _device = new ExtronDSC301HD(_serviceClient, _deviceId);
        }

        public void OnGet()
        {
            DeviceInfoCache.Available = _device.GetAvailable();
            DeviceInfoCache.Firmware = _device.GetFirmware();
            DeviceInfoCache.InputPort = _device.GetInputPort();
            DeviceInfoCache.Temperature = _device.GetTemperature();
        }

        public IActionResult OnPostScale(ScaleType scaleType, PositionType positionType)
        {
            _device.SetScale(scaleType, positionType);
            return RedirectToPage();
        }

        public IActionResult OnPostOutputRate(int width, int height, float refreshRate)
        {
            var edid = Edid.GetEdid(width, height, refreshRate);
            _device.SetOutputRate(edid);
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