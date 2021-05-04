using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace worksServer.Models.AppConfigrations
{
    public class ApplicationSettings
    {
        public string JWT_Secret { get; set; }
        public String Client_URL { get; set; }
    }
}
