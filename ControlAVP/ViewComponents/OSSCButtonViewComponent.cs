using ControllableDeviceTypes.OSSCTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlAVP.ViewComponents
{
    public class OsscButtonParameters
    {
        public CommandName CommandName { get; set; }
        public string ButtonLabel { get; set; }
        public string CssClass { get; set; }
    }

    public class OsscButtonViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(CommandName commandName, string buttonLabel, string cssClass)
        {
            await Task.FromResult(0).ConfigureAwait(false); //dummy call to make async
            return View(new OsscButtonParameters(){ CommandName = commandName, ButtonLabel = buttonLabel, CssClass = cssClass });
        }
    }
}
