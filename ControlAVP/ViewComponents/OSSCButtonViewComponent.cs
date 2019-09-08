using ControllableDeviceTypes.OSSCTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlAVP.ViewComponents
{
    public class OSSCButtonViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(CommandName commandName)
        {
            await Task.FromResult(0); //dummy call to make async
            return View(commandName);
        }
    }
}
