using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.BLL.Models.AspNetUsers
{
    public class PortalRole
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string CreatedByEmail { get; set; }
        public string CreatedById { get; set; }
    }
}
