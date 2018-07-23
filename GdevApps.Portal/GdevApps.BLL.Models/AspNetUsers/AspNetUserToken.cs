using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.BLL.Models.AspNetUsers
{
    public class AspNetUserToken
    {
        public string UserId { get; set; }
        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
