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
using ControllableDeviceTypes.RetroTink4KTypes;

namespace ControlAVP.Pages.Devices
{
    internal sealed class RetroTink4KSerialDeviceInfo
    {
        public bool Available { get; set; }
    }

    internal sealed class RetroTink4KSerialModel : PageModel
    {
        private readonly IConfiguration _configuration;

        private readonly string _connectionString;
        private readonly string _deviceId;
        private readonly ServiceClient _serviceClient;
        private readonly RetroTink4KSerial _device;

        public RetroTink4KDeviceInfo DeviceInfoCache { get; private set; } = new RetroTink4KDeviceInfo();

        public RetroTink4KSerialModel(IConfiguration configuration)
        {
            _configuration = configuration;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _device = new RetroTink4KSerial(_serviceClient, _deviceId);
        }

        public void OnGet()
        {
            DeviceInfoCache.Available = _device.GetAvailable();
        }

        public IActionResult OnPostSendCommand(CommandName commandName)
        {
            _device.SendCommand(commandName);
            return RedirectToPage();
        }
    }
}