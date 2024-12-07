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
using ControllableDeviceTypes.OSSCTypes;

namespace ControlAVP.Pages.Devices
{
    internal sealed class OSSCDeviceInfo
    {
        public bool Available { get; set; }
    }

    internal sealed class OSSCModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private readonly string _connectionString;
        private readonly string _deviceId;
        private readonly ServiceClient _serviceClient;
        private readonly OSSC _device;

        public OSSCDeviceInfo DeviceInfoCache { get; private set; } = new OSSCDeviceInfo();

        public OSSCModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _device = new OSSC(_serviceClient, _deviceId);
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

        public IActionResult OnPostLoadProfile(ProfileName profileName)
        {
            _device.LoadProfile(profileName);
            return RedirectToPage();
        }
    }
}