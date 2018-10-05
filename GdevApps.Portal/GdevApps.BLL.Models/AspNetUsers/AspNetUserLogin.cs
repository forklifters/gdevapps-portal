using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.BLL.Models.AspNetUsers
{
    public class AspNetUserLogin
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string ProviderDisplayName { get; set; }
        public string UserId { get; set; }
    }
}
