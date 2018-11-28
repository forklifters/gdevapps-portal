using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Models.ManageViewModels
{
    public class PortalUserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<PortalRoleViewModel> Roles {get;set;}
    }
}
