using System;
using System.Collections.Generic;
using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;

namespace GdevApps.DAL.DataModels.AspNetUsers.LicensedUser
{
     public partial class Products
    {
        public Products()
        {
            Licenses = new HashSet<Licenses>();
        }

        public int Id { get; set; }
        public string Type { get; set; }

        public ICollection<Licenses> Licenses { get; set; }
    }
}
