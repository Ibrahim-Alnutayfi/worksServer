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

           



            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));

            string mySqlConnection = Configuration.GetConnectionString("DevelopmentLocalConnection");
            services.AddDbContext<AuthDbContext>(options =>
            options.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection)));

            services.AddControllers();

            var key = Encoding.UTF8.GetBytes(Configuration["ApplicationSettings:JWT_Secret"].ToString());
            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x => {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = false;
                    // how to validete token once received from client-side
                    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                })
               .AddCookie( options =>
               {
                   options.LoginPath = "/auth/google-login";
               })
               .AddGoogle(options =>
               {
                   IConfigurationSection googleAuthNSection = Configuration.GetSection("Authentication:Google");
                   options.ClientId = "220493798552-le60n9p56921d83usuvchdbk7jmgoqru.apps.googleusercontent.com";
                   options.ClientSecret = "FKNmSeW2WcOvCSSS92WwpENn";
               });








            services.AddControllersWithViews();
            services.AddAuthorization();

            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequireDigit = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
                config.Password.RequiredLength = 4;
            })
               .AddEntityFrameworkStores<AuthDbContext>();

        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors(config =>
            {
                config.AllowAnyOrigin();
                config.WithOrigins(Configuration["ApplicationSettings:Client_URL"].ToString());
                config.AllowAnyHeader();
                config.AllowAnyMethod();
            });
            

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseCookiePolicy();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>{
                /*endpoints.MapControllers();*/
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });   
        }
    }
}





/**/
