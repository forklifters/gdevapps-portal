using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataModels.AspNetUsers
{
    public partial class AspNetUserTokens
    {
        public string UserId { get; set; }
        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
