using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Wedding.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        [BindProperty] // Bind on Post
        public LoginData loginData { get; set; }

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string username = _configuration.GetValue<string>(_configuration.GetSection("Credentials").GetSection("UsernameKeyName").Value);
                string password = _configuration.GetValue<string>(_configuration.GetSection("Credentials").GetSection("PasswordKeyName").Value);
                
                var isValid = (loginData.Password == password);
                if (!isValid)
                {
                    ModelState.AddModelError("", "Password is invalid.");
                    return Page();
                }
                // Create the identity from the user info
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, username));
                identity.AddClaim(new Claim(ClaimTypes.Name, username));
                // Authenticate using the identity
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = loginData.RememberMe });

                if (string.IsNullOrWhiteSpace(returnUrl) || returnUrl == @"/")
                {
                    return RedirectToPage("Index");
                }
                else
                {
                    return RedirectToPage(returnUrl);
                }
            }
            else
            {
                return Page();
            }
        }

        public class LoginData
        {
            [Required, DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; } = true;
        }

        public void OnGet()
        {
        }
    }
}