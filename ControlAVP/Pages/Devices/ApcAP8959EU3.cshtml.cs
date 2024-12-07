using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using ControllableDeviceTypes.ApcAP8959EU3Types;
using Microsoft.AspNetCore.Hosting;
using AVPCloudToDevice;

namespace ControlAVP.Pages.Devices
{
    internal sealed class OutletTableViewModel
    {
        public IEnumerable<Outlet> Outlets { get; set; }
        public string WebRootPath { get; set; }
        public IEnumerable<string> OutletConfirmation { get; set; }
        public IEnumerable<Phase> Phases { get; set; }
    }

    internal sealed class ApcAP8959EU3Model : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        private readonly string _connectionString;
        private readonly string _deviceId;
        private readonly ServiceClient _serviceClient;
        private readonly ApcAP8959EU3 _pdu;

        public ApcAP8959EU3Model(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            _connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _deviceId = _configuration.GetValue<string>("ControlAVPIoTHubDeviceId");

            _serviceClient = ServiceClient.CreateFromConnectionString(_connectionString);
            _pdu = new ApcAP8959EU3(_serviceClient, _deviceId);
        }

        public static void OnGet()
        {

        }

        public ActionResult OnGetOutletTable()
        {
            var outletTableViewModel = new OutletTableViewModel();
            outletTableViewModel.Outlets = _pdu.GetOutlets(true, false);
            outletTableViewModel.WebRootPath = _environment.WebRootPath;
            outletTableViewModel.OutletConfirmation = _configuration.GetSection("OutletConfirmation").Get<string[]>();
            outletTableViewModel.Phases = _pdu.GetPhases();

            var viewData = new ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary()) { { "OutletTableViewModel", outletTableViewModel } };
            viewData.Model = outletTableViewModel;

            PartialViewResult result = new()
            {
                ViewName = "_OutletTable",
                ViewData = viewData,
            };

            return result;
        }

        public ActionResult OnGetToggleOutletState(int id, Outlet.PowerState currentPowerState)
        {
            switch (currentPowerState)
            {
                case Outlet.PowerState.On:
                    _pdu.TurnOutletOff(id);
                    break;
                case Outlet.PowerState.Off:
                    _pdu.TurnOutletOn(id);
                    break;
            }

            var outletTableViewModel = new OutletTableViewModel();
            outletTableViewModel.Outlets = _pdu.GetOutletsWaitForPending(true, false);
            outletTableViewModel.WebRootPath = _environment.WebRootPath;
            outletTableViewModel.OutletConfirmation = _configuration.GetSection("OutletConfirmation").Get<string[]>();
            outletTableViewModel.Phases = _pdu.GetPhases();

            var viewData = new ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary()) { { "OutletTableViewModel", outletTableViewModel } };
            viewData.Model = outletTableViewModel;

            PartialViewResult result = new()
            {
                ViewName = "_OutletTable",
                ViewData = viewData,
            };

            return result;
        }

    }
}
