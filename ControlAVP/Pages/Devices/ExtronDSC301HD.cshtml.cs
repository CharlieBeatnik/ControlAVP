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

namespace ControlAVP.Pages.Devices
{
    public class ExtronDSC301HDModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;
        private ExtronDSC301HD _scaler;

        public Version Firmware { get; private set; }

        public ExtronDSC301HDModel(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _scaler = new ExtronDSC301HD(_serviceClient, _deviceId);
        }

        public void OnGet()
        {
            Firmware = _scaler.GetFirmware();
        }

        public IActionResult OnPostGetFirmware()
        {
            Firmware = _scaler.GetFirmware();
            return RedirectToPage();
        }
    }
}