using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ControlAVP
{
    public class UserDetails
    {
        public bool IsAuthenticated { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string EmailAddress { get; private set; }
        public string Name { get; private set; }

        public UserDetails(ClaimsPrincipal user)
        {
            IsAuthenticated = user.Identity.IsAuthenticated;

            if(IsAuthenticated)
            {
                FirstName = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname").Value;
                LastName = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname").Value;
                EmailAddress = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
                Name = string.Format(@"{0} {1}", FirstName, LastName);
            }
        }
    }
}
