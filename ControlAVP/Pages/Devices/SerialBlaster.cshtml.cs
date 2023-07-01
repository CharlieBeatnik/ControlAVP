using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Devices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using AVPCloudToDevice;
using ControllableDeviceTypes.SerialBlasterTypes;
using System;

namespace ControlAVP.Pages.Devices
{
    public class SerialBlasterModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;
        private SerialBlaster _device;

        uint _serialBlasterDeviceIndex;

        public SerialBlasterModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _serialBlasterDeviceIndex = 0;

            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _device = new SerialBlaster(_serviceClient, _deviceId, _serialBlasterDeviceIndex);
        }

        public void OnGet()
        {

        }

        public void OnPostConvertRawHexToNecProtocol(string rawHex, string necHex)
        {
            @ViewData["rawHex"] = $"{rawHex}";
            @ViewData["necHex"] = $"{necHex}";
        }

        public void OnPostBlastNecHex(string necHex, string rawHex)
        {
            uint command = Convert.ToUInt32(necHex, 16);
            _device.SendCommand(Protocol.Nec, command, 0);

            @ViewData["rawHex"] = $"{rawHex}";
            @ViewData["necHex"] = $"{necHex}";
        }
    }
}
