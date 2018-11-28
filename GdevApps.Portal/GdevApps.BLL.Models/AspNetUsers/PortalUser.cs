using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.BLL.Models.AspNetUsers
{
    public class PortalUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<PortalRole> Roles { get; set; }
    }
}
