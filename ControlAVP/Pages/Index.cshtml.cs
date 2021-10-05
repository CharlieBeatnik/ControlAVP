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
using ControllableDeviceTypes.SonySimpleIPTypes;
using ControllableDeviceTypes.AtenVS0801HBTypes;
using Newtonsoft.Json;
using System.Numerics;
using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace ControlAVP.Pages
{
    public class CommandInfo
    {
        public string Name { get; set; }
        public string JsonPath { get; set; }
        public string ImagePath { get; set; }
        public string DisplayName { get; set; }
    }



    public class IndexModel : PageModel
    {
        public enum Scaler
        {
            ExtronDSC301HD,
            OSSC,
            RetroTink5XPro
        }

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;
        private CommandDispatcher _cp;
        private ExtronDSC301HD _extronDSC301HD;
        private ApcAP8959EU3 _apcAP8959EU3;
        private SonySimpleIP _sonySimpleIP;
        private OSSC _ossc;
        private AtenVS0801HB _atenVS0801HB;

        private string _commandDirectory;
        private IEnumerable<Outlet> _outlets;
        private IEnumerable<string> _outletConfirmation;


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
            DirectoryInfo directoryInfo = new DirectoryInfo(_commandDirectory);

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
                using (StreamReader r = new StreamReader(file.FullName))
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
            using (StreamReader r = new StreamReader(fileFullName))
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

        public IActionResult OnPostTVInputCycle(ControllableDeviceTypes.SonySimpleIPTypes.InputPort input1, ControllableDeviceTypes.SonySimpleIPTypes.InputPort input2)
        {
            _sonySimpleIP.SetInputPort(input1);
            Thread.Sleep(TimeSpan.FromSeconds(3));
            _sonySimpleIP.SetInputPort(input2);
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

        public IActionResult OnPostOSSCProfileCycle(ProfileName profile1, ProfileName profile2)
        {
            _ossc.LoadProfile(profile1);
            Thread.Sleep(TimeSpan.FromSeconds(3));
            _ossc.LoadProfile(profile2);
            return RedirectToPage();
        }

        public IActionResult OnPostOSSCInputCycle(CommandName inputPort1, CommandName inputPort2)
        {
            _ossc.SendCommand(inputPort1);
            Thread.Sleep(TimeSpan.FromSeconds(3));
            _ossc.SendCommand(inputPort2);
            return RedirectToPage();
        }

        // Helper function to switch HDMI port to appropriate scaler
        public IActionResult OnPostScalerSelect(Scaler scaler)
        {
            ControllableDeviceTypes.AtenVS0801HBTypes.InputPort inputPort;
            switch(scaler)
            {
                case Scaler.OSSC: inputPort = ControllableDeviceTypes.AtenVS0801HBTypes.InputPort.Port2; break;
                case Scaler.ExtronDSC301HD: inputPort = ControllableDeviceTypes.AtenVS0801HBTypes.InputPort.Port3; break;
                case  Scaler.RetroTink5XPro: inputPort = ControllableDeviceTypes.AtenVS0801HBTypes.InputPort.Port8; break;
                default:
                    inputPort = ControllableDeviceTypes.AtenVS0801HBTypes.InputPort.Port1;
                    Debug.Fail($"Unknown Scaler enumeration value {scaler}");
                    break;
            }

            _atenVS0801HB.SetInputPort(inputPort);
            return RedirectToPage();
        }
    }
}
