using ControllableDeviceTypes.OSSCTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlAVP.ViewComponents
{
    public class OsscButtonViewComponent : ViewComponent
    {
        public class Parameters
        {
            public CommandName CommandName { get; set; }
            public string ButtonLabel { get; set; }
        }

        public async Task<IViewComponentResult> InvokeAsync(CommandName commandName, string buttonLabel)
        {
            await Task.FromResult(0); //dummy call to make async
            return View(new Parameters(){ CommandName = commandName, ButtonLabel = buttonLabel });
        }
    }
}
