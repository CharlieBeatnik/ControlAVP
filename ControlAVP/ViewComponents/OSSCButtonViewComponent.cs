﻿using ControllableDeviceTypes.OSSCTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlAVP.ViewComponents
{
    internal sealed class OsscButtonParameters
    {
        public CommandName CommandName { get; set; }
        public string ButtonLabel { get; set; }
        public string CssClass { get; set; }
    }

#pragma warning disable CA1515 // Consider making public types internal
    public class OsscButtonViewComponent : ViewComponent
#pragma warning restore CA1515 // Consider making public types internal
    {
        public async Task<IViewComponentResult> InvokeAsync(CommandName commandName, string buttonLabel, string cssClass)
        {
            await Task.FromResult(0).ConfigureAwait(false); //dummy call to make async
            return View(new OsscButtonParameters(){ CommandName = commandName, ButtonLabel = buttonLabel, CssClass = cssClass });
        }
    }
}
