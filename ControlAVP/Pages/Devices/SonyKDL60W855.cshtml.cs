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
    public class SonyKDL60W855Model : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;
        private SonyKDL60W855 _device;

        public SonyKDL60W855Model(IConfiguration configuration, IHostingEnvironment environment)
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

        }

        public IActionResult OnPostTurnOn()
        {
            _device.TurnOn();
            return RedirectToPage();
        }
    }
}