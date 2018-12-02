using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using PduDevice;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using PduDevice.ApcAP8959EU3Types;

namespace ControlAVP2.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public class OutletTableViewModel
        {
            public IEnumerable<Outlet> Outlets { get; set; }
            //public string WebRootPath { get; set; }
        }

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;

            string connectionString = _configuration.GetValue<string>("ControlAVPIoTHubConnectionString");
        }

        public void OnGet()
        {

        }

        public ActionResult OnGetOutletTable()
        {
            var outletTableViewModel = new OutletTableViewModel();
            outletTableViewModel.Outlets = GetPDUState();
            //outletTableViewModel.WebRootPath = _environment.WebRootPath;

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
            //outletTableViewModel.WebRootPath = _environment.WebRootPath;

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
            //var response = Task.Run(async () => await InvokeDeviceMethodAsync(_serviceClient, _deviceId, methodInvocation)).Result;

            //if (response != null)
            //{
            //    Console.WriteLine("Response status: {0}, payload:", response.Status);
            //    Console.WriteLine(response.GetPayloadAsJson());
            //}

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
