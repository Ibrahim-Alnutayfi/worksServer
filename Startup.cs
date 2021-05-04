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



using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using worksServer.Models.AppConfigrations;

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

            services.AddIdentity<IdentityUser, IdentityRole>(config =>{
                config.Password.RequireDigit = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
                config.Password.RequiredLength = 4;
            })
              .AddEntityFrameworkStores<AuthDbContext>();




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
                    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });           
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()){
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(config =>
            {
                config.WithOrigins(Configuration["ApplicationSettings:Client_URL"].ToString());
                config.AllowAnyHeader();
                config.AllowAnyMethod();
            });

            app.UseEndpoints(endpoints =>{
                endpoints.MapControllers();
            });   
        }
    }
}
