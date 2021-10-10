using ControllableDeviceTypes.RetroTink5xProTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlAVP.ViewComponents
{
    public class RetroTink5xProButtonParameters
    {
        public CommandName CommandName { get; set; }
        public string ButtonLabel { get; set; }
        public string CssClass { get; set; }
        public string CssFaIcon { get; set; }
    }

    public class RetroTink5xProButtonViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(CommandName commandName, string buttonLabel, string cssClass, string cssFaIcon)
        {
            await Task.FromResult(0).ConfigureAwait(false); //dummy call to make async
            return View(new RetroTink5xProButtonParameters() { CommandName = commandName, ButtonLabel = buttonLabel, CssClass = cssClass, CssFaIcon = cssFaIcon });
        }
    }
}
