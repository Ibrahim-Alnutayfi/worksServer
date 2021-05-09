using worksServer.Models.AppConfigrations;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using worksServer.Models;
using System.Net.Mail;
using System.Linq;
using System.Text;
using System;


namespace worksServer.Controllers {


    [AllowAnonymous, Route("auth")]
    [ApiController]
    public class AuthController : Controller{


        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationSettings _appSettings;


        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<ApplicationSettings> appSettings) {

            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
        }


        [Route("Register")]
        [HttpPost]
        public async Task<object> Register(UserRegister data){

            User user = new User{
                UserName = data.UserName,
                Email = data.Email,
                FirstName = data.FirstName,
                LastName = data.LastName,
            };

            var result = await _userManager.CreateAsync(user, data.Password);

            if (result.Succeeded)
            {
                EmailVirefication(user);
                return Ok(new { succeeded = true, EmailVirefication = "We just send a vierfication email to you please confirm it" });
            }
               
            else
                return BadRequest(new { succeeded = false, errors = result.Errors.ToArray() });
        }


        [Route("Login")]
        [HttpPost]
        public async Task<ActionResult> Login(UserLogin loginRequest){

            var userRequst = new User { UserName = loginRequest.UserName };
            var user = await _userManager.FindByNameAsync(userRequst.UserName);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

            if (user != null && isPasswordValid){

                var tokenDescriptor = new SecurityTokenDescriptor{

                    Subject = new ClaimsIdentity(new[] {

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




        public async void EmailVirefication(User user){

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            string link = Url.Action(nameof(VerifyEmail), "Auth", new { userId = user.Id, code },Request.Scheme,Request.Host.ToString());
            string sender = "xxxxx";
            string password = "xxxxx";
            string receiver = user.Email;
            string subject = "Email Verfication";
            string firstName = user.FirstName;
            
            SendEmail(sender, password, receiver, subject, link, firstName);
        }

        [Route("SendEmail")]
        [HttpGet]
        public void SendEmail(string sender, string password, string receiver,string subject, string link, string firstName) {

            MailAddress objFrom = new MailAddress(sender);
            MailAddress objTo = new MailAddress(receiver);
            MailMessage msgMail = new MailMessage(objFrom, objTo);

            var time = DateTime.Now.Hour;
            var welcomingMessage = time < 12 ? "Morning" : "Hi";

            msgMail.Subject = subject;
            msgMail.Body =  $"{welcomingMessage} {firstName},\n\nPlease click the link below to verify your email.\n\n {link}\n\nBest regards,\nSupport Team." ;
            SmtpClient objSMTP = new SmtpClient("smtp.gmail.com", 587);
            objSMTP.UseDefaultCredentials = false;
            objSMTP.Credentials = new System.Net.NetworkCredential(sender, password);
            objSMTP.EnableSsl = true;
            objSMTP.Send(msgMail);
        }

        [Route("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            IdentityUser user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest();

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
                return Ok(new { Succeeded = true });

            return BadRequest(new { succeeded = false ,errors = result.Errors.ToArray() });
        }
    }
}








