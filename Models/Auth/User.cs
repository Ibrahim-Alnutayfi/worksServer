using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace worksServer.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }



        public class ApplicationDbContext : IdentityDbContext<User>
        {
            public ApplicationDbContext()
                : base()
            {
            }
            public static AuthDbContext Create()
            {
                return new AuthDbContext();
            }
        }
    }
   
}
