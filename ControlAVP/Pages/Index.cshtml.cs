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
using Newtonsoft.Json;
using System.Numerics;

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
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private string _connectionString;
        private string _deviceId;
        private ServiceClient _serviceClient;
        private CommandProcessor _cp;
        private ExtronDSC301HD _extronDSC301HD;
        private OSSC _ossc;

        private string _commandDirectory;

        public IList<CommandInfo> CommandInfos { get; private set; }
        public bool ExtronDSC301HDAvailable { get; private set; }

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
            _cp = new CommandProcessor(_serviceClient, _deviceId);
            _extronDSC301HD = new ExtronDSC301HD(_serviceClient, _deviceId);
            _ossc = new OSSC(_serviceClient, _deviceId);

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

            ExtronDSC301HDAvailable = _extronDSC301HD.GetAvailable();

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

        public IActionResult OnPostCommandProcessorExecute(string fileFullName)
        {
            using (StreamReader r = new StreamReader(fileFullName))
            {
                string json = r.ReadToEnd();
                _cp.Execute(json);
            }

            return RedirectToPage();
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

        public IActionResult OnPostOSSCSendCommand(CommandName commandName)
        {
            _ossc.SendCommand(commandName);
            return RedirectToPage();
        }
    }
}
