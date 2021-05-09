using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using worksServer.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;



using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using worksServer.Models.AppConfigrations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;


namespace worksServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration){
            Configuration = configuration;
        }
          

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services){

            services.AddControllers();
            services.AddControllersWithViews();
            services.AddAuthorization();


            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {

                config.Password.RequireDigit = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
                config.Password.RequiredLength = 4;
                config.User.AllowedUserNameCharacters = "_-abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQURSTUVWXYZ";
                config.User.RequireUniqueEmail = true;
                config.SignIn.RequireConfirmedEmail = true;
            })
               .AddEntityFrameworkStores<AuthDbContext>()
               .AddDefaultTokenProviders();


            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));
            string mySqlConnection = Configuration.GetConnectionString("DevelopmentLocalConnection");
            services.AddDbContext<AuthDbContext>(options => {

                options.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection));
            });


            var key = Encoding.UTF8.GetBytes(Configuration["ApplicationSettings:JWT_Secret"]);
            services.AddAuthentication(config =>
            {
                config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(config =>
                {
                    config.RequireHttpsMetadata = false;
                    config.SaveToken = false;
                    config.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                })

               .AddCookie(config =>
               {
                   config.LoginPath = "/auth/google-login";
               })

               .AddGoogle(config =>
               {
                   IConfigurationSection googleAuthNSection = Configuration.GetSection("Authentication:Google");
                   config.ClientId = Configuration["GoogleAuthSettings:ClientId"];
                   config.ClientSecret = Configuration["GoogleAuthSettings:ClientSecret"];
               });

        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else{
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseCors(config => {

                config.AllowAnyOrigin();
                config.AllowAnyHeader();
                config.AllowAnyMethod();
            });
            

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseCookiePolicy();
            app.UseAuthorization();


            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });   
        }
    }
}