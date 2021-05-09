using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using worksServer.Models;
using System.Web.Http;
using System.Linq;
using System.Text;
using System;

namespace worksServer.Controllers
{
    [AllowAnonymous, Microsoft.AspNetCore.Mvc.Route("ExternalAuth")]
    public class ExternalAuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ExternalAuthController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }


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
                .Claims.Select(claim => new
                {
                    claim.Issuer,
                    claim.OriginalIssuer,
                    claim.Type,
                    claim.Value
                }).ToList();

            string Issuer = claims[0].Issuer;
            string id = claims[0].Value;
            string name = claims[1].Value;
            string email = claims[4].Value;
            string firstName = name.Substring(0, name.IndexOf(" "));
            string lastName = name.Substring(name.IndexOf(" ") + 1);


            User user = await GetOrCreateExternalLoginUser(Issuer, id, firstName, lastName, email);
            if (user == null)
                return Json(new { Succeeded = false });
            else{
                string token = await GenerateToken(user);
                return new JsonResult(token);
            }
        }


        public async Task<string> GenerateToken(User user)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(new[] {
                        new Claim("UserID", user.Id.ToString())
                    }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567890123456")), SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);
            return token;
        }



        public async Task<User> GetOrCreateExternalLoginUser(string provider, string key, string firstName, string lastName, string email)
        {
            lastName = lastName.First().ToString().ToUpper() + lastName.Substring(1);
            IdentityUser user = await _userManager.FindByLoginAsync(provider, key);
            if (user != null)
                return (User)user;
            user = await _userManager.FindByEmailAsync(email);
            if (user == null) {
                user = new User {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    UserName = firstName + lastName
                };
                await _userManager.CreateAsync(user);
            }
            var info = new UserLoginInfo(provider, key, provider.ToUpperInvariant());
            var result = await _userManager.AddLoginAsync(user, info);
            if (result.Succeeded)
                return (User)user;

            return null;
        }

    }
}