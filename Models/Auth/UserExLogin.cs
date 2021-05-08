using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace worksServer.Models.Auth
{
    public class UserExLogin
    {

        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string ProviderDisplayName { get; set; }
        public string UserId { get; set; }

    }
}
