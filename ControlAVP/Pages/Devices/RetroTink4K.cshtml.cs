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
    internal sealed class RetroTink4KDeviceInfo
    {
        public bool Available { get; set; }
    }

    internal sealed class RetroTink4KModel : PageModel
    {
        private readonly IConfiguration _configuration;

        private readonly string _connectionString;
        private readonly string _deviceId;
        private readonly ServiceClient _serviceClient;
        private readonly RetroTink4K _device;

        public RetroTink4KDeviceInfo DeviceInfoCache { get; private set; } = new RetroTink4KDeviceInfo();

        public RetroTink4KModel(IConfiguration configuration)
        {
            _configuration = configuration;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _device = new RetroTink4K(_serviceClient, _deviceId);
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

        public IActionResult OnPostLoadProfileQuick(ProfileName profileName)
        {
            _device.LoadProfileQuick(profileName);
            return RedirectToPage();
        }

        public IActionResult OnPostLoadProfile(uint directoryIndex, uint profileIndex)
        {
            _device.LoadProfile(directoryIndex, profileIndex);
            return RedirectToPage();
        }

        public IActionResult OnPostTogglePower()
        {
            _device.TogglePower();
            return RedirectToPage();
        }
    }
}