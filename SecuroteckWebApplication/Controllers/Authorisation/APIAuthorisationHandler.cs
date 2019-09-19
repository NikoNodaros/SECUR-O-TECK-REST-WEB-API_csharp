using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SecuroteckWebApplication.Models;
namespace SecuroteckWebApplication.Controllers
{
    public class APIAuthorisationHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
        {

            #region Task5
            // TODO:  Find if a header ‘ApiKey’ exists, and if it does, check the database to determine if the given API Key is valid
            //        Then authorise the principle on the current thread using a claim, claimidentity and claimsprinciple
            if (request.Headers.TryGetValues("ApiKey", out IEnumerable<string> headerValues))
            {
                if (UserDatabaseAccess.UserExists(new Guid(headerValues.FirstOrDefault()), out User user))
                {
                    UserDatabaseAccess.Log("User Requested " + request.RequestUri.ToString(), headerValues.First());
                    var claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                    claims.Add(new Claim(ClaimTypes.Role, user.Role.ToString()));
                    claims.Add(new Claim(ClaimTypes.Authentication, user.ApiKey));

                    ClaimsIdentity identity = new ClaimsIdentity("ApiKey");
                    identity.AddClaims(claims);

                    ClaimsPrincipal principle = new ClaimsPrincipal();
                    principle.AddIdentity(new ClaimsIdentity(identity));

                    Thread.CurrentPrincipal = principle;
                }
            }
            return base.SendAsync(request, cancellationToken);
            #endregion
        }
    }
}