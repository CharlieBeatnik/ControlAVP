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
    public class OSSCModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;
        private OSSC _device;

        public class DeviceInfo
        {
            public bool Available;
        }
        public DeviceInfo DeviceInfoCache { get; private set; } = new DeviceInfo();

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
    }
}