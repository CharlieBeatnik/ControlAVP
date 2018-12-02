using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PduDevice;
using System.Web;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Devices;
using PduDevice.ApcAP8959EU3Types;

namespace ControlAVP.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;
        private ServiceClient _serviceClient;

        private readonly string _deviceId = "gamesroom";

        public class OutletTableViewModel
        {
            public IEnumerable<Outlet> Outlets { get; set; }
            public string WebRootPath { get; set; }
        }

        private void UpdatePduComms()
        {
        }

        public IndexModel(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            string connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
            _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
        }

        public UserDetails AuthenticatedUser { get; private set; }

        public void OnGet()
        {
            AuthenticatedUser = new UserDetails(User);
        }

        public ActionResult OnGetOutletTable()
        {
            UpdatePduComms();

            var outletTableViewModel = new OutletTableViewModel();
            outletTableViewModel.Outlets = GetPDUState();
            outletTableViewModel.WebRootPath = _environment.WebRootPath;

            var viewData = new ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary()) { { "OutletTableViewModel", outletTableViewModel } };
            viewData.Model = outletTableViewModel;

            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_OutletTable",
                ViewData = viewData,
            };

            return result;
        }

        public ActionResult OnGetToggleOutletState(int id, Outlet.PowerState currentPowerState)
        {
            UpdatePduComms();

            switch (currentPowerState)
            {
                case Outlet.PowerState.On:
                    //_pduComms.TurnOutletOff(id); ANDREWDENN_TODO
                    break;
                case Outlet.PowerState.Off:
                    //_pduComms.TurnOutletOn(id); ANDREWDENN_TODO
                    break;
            }

            var outletTableViewModel = new OutletTableViewModel();
            outletTableViewModel.Outlets = new List<Outlet>(); //ANDREWDENN_TODO_pduComms.GetOutletsWaitForPending(new List<int> { id });
            outletTableViewModel.WebRootPath = _environment.WebRootPath;

            var viewData = new ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary()) { { "OutletTableViewModel", outletTableViewModel } };
            viewData.Model = outletTableViewModel;

            PartialViewResult result = new PartialViewResult()
            {
                ViewName = "_OutletTable",
                ViewData = viewData,
            };

            return result;
        }

        public IEnumerable<Outlet> GetPDUState()
        {
            var methodInvocation = new CloudToDeviceMethod("GetPDUState") { ResponseTimeout = TimeSpan.FromSeconds(30) };

            // Create JSON message
            var payload = new
            {
            };
            var payloadString = JsonConvert.SerializeObject(payload);
            methodInvocation.SetPayloadJson(payloadString);

            // Invoke the direct method asynchronously and get the response from the device.
            //var response = Task.Run(async () => await InvokeDeviceMethodAsync(_serviceClient, _deviceId, methodInvocation)).Result;

            //if (response != null)
            //{
            //    string json = response.GetPayloadAsJson();
            //    return JsonConvert.DeserializeObject<List<Outlet>>(json);
            //}

            return null;
        }


        public IActionResult OnPostGetPDUState()
        {
            var methodInvocation = new CloudToDeviceMethod("GetPDUState") { ResponseTimeout = TimeSpan.FromSeconds(30) };

            // Create JSON message
            var payload = new
            {
            };
            var payloadString = JsonConvert.SerializeObject(payload);
            methodInvocation.SetPayloadJson(payloadString);

            // Invoke the direct method asynchronously and get the response from the device.
            var response = Task.Run(async () => await InvokeDeviceMethodAsync(_serviceClient, _deviceId, methodInvocation)).Result;

            if (response != null)
            {
                Console.WriteLine("Response status: {0}, payload:", response.Status);
                Console.WriteLine(response.GetPayloadAsJson());
            }

            return RedirectToPage();
        }

        private static async Task<CloudToDeviceMethodResult> InvokeDeviceMethodAsync(ServiceClient serviceClient, string deviceId, CloudToDeviceMethod cloudToDeviceMethod)
        {
            try
            {
                var result = await serviceClient.InvokeDeviceMethodAsync(deviceId, cloudToDeviceMethod);
                return result;
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }
    }
}