using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using System.ComponentModel.DataAnnotations;

namespace ControlAVP.Pages.Devices
{
    public class SerialBlasterModel : PageModel
    {
        public void OnGet()
        {
        }

        public void OnPostBlastRawHex(string rawHex)
        {
            @ViewData["rawHex"] = $"{rawHex}";
        }
    }
}
