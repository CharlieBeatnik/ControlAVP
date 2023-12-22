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
using System.Numerics;

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
        public bool? Freeze { get; set; }

        public Vector2? InputResolution { get; set; }
    }

    public class ExtronDSC301HDModel : PageModel
    {
        private readonly IConfiguration _configuration;

        private readonly string _connectionString;
        private readonly string _deviceId;
        private readonly ServiceClient _serviceClient;
        private readonly ExtronDSC301HD _device;

        public ExtronDSC301HDDeviceInfo DeviceInfoCache { get; private set; } = new ExtronDSC301HDDeviceInfo();

        public float PaddingX { get; private set; }
        public float PaddingY { get; private set; }

        public ExtronDSC301HDModel(IConfiguration configuration)
        {
            _configuration = configuration;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _device = new ExtronDSC301HD(_serviceClient, _deviceId);
        }

        public void OnGet(float paddingX, float paddingY)
        {
            DeviceInfoCache.Available = _device.GetAvailable();
            DeviceInfoCache.Firmware = _device.GetFirmware();
            DeviceInfoCache.InputPort = _device.GetInputPort();
            DeviceInfoCache.Temperature = _device.GetTemperature();
            DeviceInfoCache.DetailFilter = _device.GetDetailFilter();
            DeviceInfoCache.Brightness = _device.GetBrightness();
            DeviceInfoCache.Contrast = _device.GetContrast();
            DeviceInfoCache.Freeze = _device.GetFreeze();
            DeviceInfoCache.InputResolution = _device.GetInputResolution();

            PaddingX = paddingX;
            PaddingY = paddingY;
        }

        public IActionResult OnPostSetScale(ScaleType scaleType, PositionType positionType, AspectRatio aspectRatio, float paddingX = 0, float paddingY = 0)
        {
            _device.SetScale(scaleType, positionType, aspectRatio, new Vector2() { X = paddingX, Y = paddingY });
            return RedirectToPage(new { paddingX, paddingY });
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

        public IActionResult OnPostSetFreeze(bool freeze)
        {
            _device.SetFreeze(freeze);
            return RedirectToPage();
        }
    }
}