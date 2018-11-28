using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Models.ManageViewModels
{
    public class PortalRoleViewModel
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string CreatedByEmail { get; set; }
        public string CreatedById { get; set; }
    }
}
