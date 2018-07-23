using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataContexts.GradebookLicense.Model
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
