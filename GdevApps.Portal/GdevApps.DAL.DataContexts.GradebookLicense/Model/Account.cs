using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataContexts.GradebookLicense.Model
{
    public partial class Account
    {
        public Account()
        {
            Licenses = new HashSet<Licenses>();
        }

        public int Id { get; set; }
        public string Type { get; set; }

        public ICollection<Licenses> Licenses { get; set; }
    }
}
