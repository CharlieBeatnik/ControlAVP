using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wedding
{
    public class LogoutModel : PageModel
    {
        public async Task<ActionResult> OnGet()
        {
            await HttpContext.SignOutAsync();
            return RedirectToPage("Index");
        }
    }
}