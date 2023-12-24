using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AVPCloudToDevice;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Mvc;
using ControllableDeviceTypes.ExtronDSC301HDTypes;
using ControllableDeviceTypes.OSSCTypes;
using ControllableDeviceTypes.ApcAP8959EU3Types;
using Newtonsoft.Json;
using System.Numerics;
using System;
using System.Linq;

namespace ControlAVP.Pages
{
    public class CommandInfo
    {
        public string Name { get; set; }
        public string JsonPath { get; set; }
        public string ImagePath { get; set; }
        public string DisplayName { get; set; }
    }

    public enum Scaler
    {
        RetroTink4K,
        OSSC,
        ExtronDSC301HD
    }

    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private readonly string _connectionString;
        private readonly string _deviceId;
        private readonly ServiceClient _serviceClient;
        private readonly CommandDispatcher _cp;
        private readonly ExtronDSC301HD _extronDSC301HD;
        private readonly ApcAP8959EU3 _apcAP8959EU3;
        private readonly SonySimpleIP _sonySimpleIP;
        private readonly OSSC _ossc;
        private readonly AtenVS0801HB _atenVS0801HB;

        private readonly string _commandDirectory;
        private readonly IEnumerable<Outlet> _outlets;
        private readonly IEnumerable<string> _outletConfirmation;


        public IList<CommandInfo> CommandInfos { get; private set; }
        public bool RackDevicesAvailable { get; private set; }
        public bool ScalerCardVisible { get; private set; }
        public bool OsscCardVisible { get; private set; }

        public IndexModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(environment != null);

            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _cp = new CommandDispatcher(_serviceClient, _deviceId);
            _extronDSC301HD = new ExtronDSC301HD(_serviceClient, _deviceId);
            _apcAP8959EU3 = new ApcAP8959EU3(_serviceClient, _deviceId);
            _sonySimpleIP = new SonySimpleIP(_serviceClient, _deviceId);
            _ossc = new OSSC(_serviceClient, _deviceId);
            _atenVS0801HB = new AtenVS0801HB(_serviceClient, _deviceId, 0);

            _outlets = _apcAP8959EU3.GetOutlets();
            _outletConfirmation = _configuration.GetSection("OutletConfirmation").Get<string[]>();

            _commandDirectory = Path.Combine(_environment.WebRootPath, "commands");
        }

        public void OnGet(bool scalerCardVisible, bool osscCardVisible)
        {
            DirectoryInfo directoryInfo = new(_commandDirectory);

            CommandInfos = new List<CommandInfo>();
            foreach (var file in directoryInfo.GetFiles("*.json"))
            {
                string fileNameNoExtension = Path.GetFileNameWithoutExtension(file.Name);

                string relativeImagePath = string.Format("../images/outlets/large/{0}.png", fileNameNoExtension);
                string absoluteImagePath = Path.Combine(_environment.WebRootPath, @"images\outlets\large", fileNameNoExtension + ".png");

                if(!System.IO.File.Exists(absoluteImagePath))
                {
                    relativeImagePath = "../images/outlets/large/empty.png";
                }

                //Get display name from Json instead of replying on the file name
                string displayName = string.Empty;
                using (StreamReader r = new(file.FullName))
                {
                    string json = r.ReadToEnd();
                    dynamic parsed = JsonConvert.DeserializeObject(json);
                    displayName = parsed.DisplayName;
                }

                CommandInfos.Add(new CommandInfo
                {
                    Name = fileNameNoExtension,
                    JsonPath = file.FullName,
                    ImagePath = relativeImagePath,
                    DisplayName = displayName
                });
            }

            if (_outlets != null)
            {
                var rackOutlet = _outlets.FirstOrDefault(o => o.Name == "Rack");
                RackDevicesAvailable = rackOutlet?.State == Outlet.PowerState.On;
            }

            ScalerCardVisible = scalerCardVisible;
            OsscCardVisible = osscCardVisible;
        }

        //Override redirect to page to deal with common parameters
        public override RedirectToPageResult RedirectToPage()
        {
            bool scalerCardVisible = false;
            bool osscCardVisible = false;

            if (Request.Query.TryGetValue("scalerCardVisible", out var scalerCardVisibleStr))
            {
                scalerCardVisible = bool.Parse(scalerCardVisibleStr);
            }

            if (Request.Query.TryGetValue("osscCardVisible", out var osscCardVisibleStr))
            {
                osscCardVisible = bool.Parse(osscCardVisibleStr);
            }

            return base.RedirectToPage(new { scalerCardVisible, osscCardVisible });
        }

        public IActionResult OnPostCommandProcessorExecute(string fileFullName, string displayName, string imagePath)
        {
            Guid id = Guid.NewGuid();
            using (StreamReader r = new(fileFullName))
            {
                string json = r.ReadToEnd();
                _cp.Dispatch(json, id);
            }

            return RedirectToPage("TailCommandProcessor", new { id, displayName, imagePath });
        }

        public IActionResult OnPostSetScale(ScaleType scaleType, PositionType positionType, AspectRatio aspectRatio, float paddingX = 0, float paddingY = 0)
        {
            _extronDSC301HD.SetScale(scaleType, positionType, aspectRatio, new Vector2() { X = paddingX, Y = paddingY });
            return RedirectToPage();
        }

        public IActionResult OnPostSetDetailFilter(int value)
        {
            _extronDSC301HD.SetDetailFilter(value);
            return RedirectToPage();
        }

        public IActionResult OnPostSetContrast(int value)
        {
            _extronDSC301HD.SetContrast(value);
            return RedirectToPage();
        }

        // Turn off all devices not on the exclusion list, including the TV
        public IActionResult OnPostPowerOff()
        {
            var outlets = _apcAP8959EU3.GetOutlets();

            if(outlets != null)
            {
                foreach (var outlet in outlets.Where(o =>
                    !_outletConfirmation.Contains(o.Name) &&
                    o.State == Outlet.PowerState.On))
                {
                    _apcAP8959EU3.TurnOutletOff(outlet.Id);
                }
            }

            _sonySimpleIP.TurnOff();

            return RedirectToPage();
        }

        public IActionResult OnPostOSSCSendCommand(CommandName commandName)
        {
            _ossc.SendCommand(commandName);
            return RedirectToPage();
        }

        public IActionResult OnPostSetScaler(Scaler scaler)
        {
            switch(scaler)
            {
                case Scaler.RetroTink4K:
                    _sonySimpleIP.SetInputPort(ControllableDeviceTypes.SonySimpleIPTypes.InputPort.Hdmi2);
                    break;
                case Scaler.OSSC:
                    _sonySimpleIP.SetInputPort(ControllableDeviceTypes.SonySimpleIPTypes.InputPort.Hdmi1);
                    _atenVS0801HB.SetInputPort(ControllableDeviceTypes.AtenVS0801HBTypes.InputPort.Port1);
                    break;
                case Scaler.ExtronDSC301HD:
                    _sonySimpleIP.SetInputPort(ControllableDeviceTypes.SonySimpleIPTypes.InputPort.Hdmi1);
                    _atenVS0801HB.SetInputPort(ControllableDeviceTypes.AtenVS0801HBTypes.InputPort.Port2);
                    break;
            }

            return RedirectToPage();
        }
    }
}
