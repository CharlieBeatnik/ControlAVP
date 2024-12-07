using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ControlAVP.Pages
{
    internal sealed class LogoutModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            await HttpContext.SignOutAsync().ConfigureAwait(false);
            return RedirectToPage("Index");
        }
    }
}