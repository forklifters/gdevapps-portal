using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataModels.AspNetUsers
{
    public class AspNetUserTokens
    {
        public string UserId { get; set; }
        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType()) return false;

            AspNetUserTokens p = (AspNetUserTokens)obj;
            return (this.UserId == p.UserId) && (this.LoginProvider == p.LoginProvider) && (this.Name == p.Name);
        }
    }
}
