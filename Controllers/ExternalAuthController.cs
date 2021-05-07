using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;


namespace worksServer.Controllers
{
    [AllowAnonymous, Microsoft.AspNetCore.Mvc.Route("ExternalAuth")]
    public class ExternalAuthController : Controller
    {

        [Microsoft.AspNetCore.Mvc.Route("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        [Microsoft.AspNetCore.Mvc.Route("google-response")]
        public async Task<IActionResult> GoogleResponse(HttpActionContext actionContext)
        {   
            var result = await HttpContext.AuthenticateAsync("Google");

            var claims = result.Principal.Identities.FirstOrDefault()
                .Claims.Select(claim => new {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value
                });

            return Json(claims);
        }

    }
}
