using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using worksServer.Models;
using worksServer.Models.AppConfigrations;



namespace worksServer.Controllers {

    
    [AllowAnonymous, Route("auth")]
    [ApiController]
    public class AuthController : Controller
    {

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationSettings _appSettings;
        

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<ApplicationSettings> appSettings)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
        }


        [Route("Register")]
        [HttpPost]
        public async Task<object> Register(UserRegister data)
        {
            var user = new User {
                UserName = data.UserName,
                Email = data.Email,
                FirstName = data.FirstName,
                LastName = data.LastName,
            };           
            
            var result = await _userManager.CreateAsync(user, data.Password);

            if (result.Succeeded)
                return Ok(new { succeeded = true });
            else
                return BadRequest(new { succeeded = false, errors = result.Errors.ToArray() });
        }


        [Route("Login")]
        [HttpPost]
        public async Task<ActionResult> Login(UserLogin loginRequest)
        {
            var userRequst = new User { UserName = loginRequest.UserName };
            var user = await _userManager.FindByNameAsync(userRequst.UserName);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

            if (user != null && isPasswordValid)
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity( new[] {
                        new Claim("UserID", user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha512Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);

                return Ok(new { succeeded = true, token });
            }
            else
                return BadRequest(new { message = "Username or password is incorrect" });
        }
    }
}








