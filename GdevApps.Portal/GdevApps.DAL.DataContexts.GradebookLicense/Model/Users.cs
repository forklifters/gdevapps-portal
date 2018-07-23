using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataContexts.GradebookLicense.Model
{
    public partial class Users
    {
        public Users()
        {
            Licenses = new HashSet<Licenses>();
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int Role { get; set; }

        public Roles RoleNavigation { get; set; }
        public ICollection<Licenses> Licenses { get; set; }
    }
}
