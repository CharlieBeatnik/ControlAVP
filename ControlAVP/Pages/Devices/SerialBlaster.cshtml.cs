using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Devices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using AVPCloudToDevice;
using ControllableDeviceTypes.SerialBlasterTypes;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ControlAVP.Pages.Devices
{
    internal sealed class SerialBlasterModel : PageModel
    {
        private readonly IConfiguration _configuration;

        private readonly string _connectionString;
        private readonly string _deviceId;
        private readonly ServiceClient _serviceClient;
        private readonly SerialBlaster _device;

        readonly uint _serialBlasterDeviceIndex;

        public SerialBlasterModel(IConfiguration configuration)
        {
            _serialBlasterDeviceIndex = 0;

            _configuration = configuration;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _device = new SerialBlaster(_serviceClient, _deviceId, _serialBlasterDeviceIndex);
        }

        public void OnPostBlastNecHex(string necHex, string rawHex)
        {
            uint command = Convert.ToUInt32(necHex, 16);
            _device.SendCommand(Protocol.Nec, command, 0);

            @ViewData["necHex"] = $"{necHex}";
        }
    }
}
