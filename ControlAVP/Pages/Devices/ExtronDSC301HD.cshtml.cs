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
    public class ExtronDSC301HDDeviceInfo
    {
        public bool Available { get; set; }
        public Version Firmware { get; set; }
        public InputPort? InputPort { get; set; }
        public float? Temperature { get; set; }
        public int? DetailFilter { get; set; }
        public int? Brightness { get; set; }
        public int? Contrast { get; set; }
    }

    public class ExtronDSC301HDModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;
        private ExtronDSC301HD _device;

        public ExtronDSC301HDDeviceInfo DeviceInfoCache { get; private set; } = new ExtronDSC301HDDeviceInfo();

        public ExtronDSC301HDModel(IConfiguration configuration, IWebHostEnvironment environment)
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
            DeviceInfoCache.DetailFilter = _device.GetDetailFilter();
            DeviceInfoCache.Brightness = _device.GetBrightness();
            DeviceInfoCache.Contrast = _device.GetContrast();
        }

        public IActionResult OnPostSetScale(ScaleType scaleType, PositionType positionType, AspectRatio aspectRatio)
        {
            _device.SetScale(scaleType, positionType, aspectRatio);
            return RedirectToPage();
        }

        public IActionResult OnPostSetOutputRate(int width, int height, float refreshRate)
        {
            var edid = Edid.GetEdid(width, height, refreshRate);
            _device.SetOutputRate(edid);
            return RedirectToPage();
        }

        public IActionResult OnPostSetInput(InputPort inputPort)
        {
            _device.SetInputPort(inputPort);
            return RedirectToPage();
        }

        public IActionResult OnPostSetDetailFilter(int value)
        {
            _device.SetDetailFilter(value);
            return RedirectToPage();
        }

        public IActionResult OnPostSetBrightness(int value)
        {
            _device.SetBrightness(value);
            return RedirectToPage();
        }

        public IActionResult OnPostSetContrast(int value)
        {
            _device.SetContrast(value);
            return RedirectToPage();
        }
    }
}