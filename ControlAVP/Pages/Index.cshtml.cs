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

namespace ControlAVP.Pages
{
    public class CommandInfo
    {
        public string Name { get; set; }
        public string JsonPath { get; set; }
        public string ImagePath { get; set;  }
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

        private string _commandDirectory;

        public List<CommandInfo> CommandInfos { get; private set; }
        public bool ExtronDSC301HDAvailable { get; private set; }

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

            _commandDirectory = Path.Combine(_environment.WebRootPath, "commands");
        }

        public void OnGet()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_commandDirectory);

            CommandInfos = new List<CommandInfo>();
            foreach (var file in directoryInfo.GetFiles("*.json"))
            {
                string name = Path.GetFileNameWithoutExtension(file.Name);

                string relativeImagePath = string.Format("../images/outlets/large/{0}.png", name);
                string absoluteImagePath = Path.Combine(_environment.WebRootPath, @"images\outlets\large", name + ".png");

                if(!System.IO.File.Exists(absoluteImagePath))
                {
                    relativeImagePath = "../images/outlets/large/empty.png";
                }

                CommandInfos.Add(new CommandInfo
                {
                    Name = name,
                    JsonPath = file.FullName,
                    ImagePath = relativeImagePath
                });
            }

            ExtronDSC301HDAvailable = _extronDSC301HD.GetAvailable();
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

        public IActionResult OnPostScale(ScaleType scaleType, PositionType positionType)
        {
            _extronDSC301HD.SetScale(scaleType, positionType);
            return RedirectToPage();
        }
    }
}
