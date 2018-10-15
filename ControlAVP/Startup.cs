using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace ControlAVP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddAuthentication();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Code from this post: https://stackoverflow.com/questions/42260708/azure-apps-easyauth-claims-with-net-core/42270669#42270669
            // Looks at HTTP headers and runs a get on /.auth/me to get full user data
            app.Use(async (context, next) =>
            {
                // Create a user on current thread from provided header
                if (context.Request.Headers.ContainsKey("X-MS-CLIENT-PRINCIPAL-ID"))
                {
                    // Read headers from Azure
                    var azureAppServicePrincipalIdHeader = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"][0];
                    var azureAppServicePrincipalNameHeader = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"][0];

                    //invoke /.auth/me
                    var cookieContainer = new CookieContainer();
                    HttpClientHandler handler = new HttpClientHandler()
                    {
                        CookieContainer = cookieContainer
                    };
                    string uriString = $"{context.Request.Scheme}://{context.Request.Host}";
                    foreach (var c in context.Request.Cookies)
                    {
                        cookieContainer.Add(new Uri(uriString), new Cookie(c.Key, c.Value));
                    }
                    string jsonResult = string.Empty;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        var res = await client.GetAsync($"{uriString}/.auth/me");
                        jsonResult = await res.Content.ReadAsStringAsync();
                    }

                    //parse json
                    var obj = JArray.Parse(jsonResult);
                    string user_id = obj[0]["user_id"].Value<string>(); //user_id

                    // Create claims id
                    List<Claim> claims = new List<Claim>();
                    foreach (var claim in obj[0]["user_claims"])
                    {
                        claims.Add(new Claim(claim["typ"].ToString(), claim["val"].ToString()));
                    }

                    // Set user in current context as claims principal
                    var identity = new GenericIdentity(azureAppServicePrincipalIdHeader);
                    identity.AddClaims(claims);

                    // Set current thread user to identity
                    context.User = new GenericPrincipal(identity, null);
                };

                await next.Invoke();
            });

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseMvc();
            app.UseAuthentication();

        }
    }
}
