using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Models.ManageViewModels
{
    public class GeneralPortalUserViewModel: PortalUserViewModel
    {
        public List<PortalRoleViewModel> Roles {get;set;} = new List<PortalRoleViewModel>();
    }
}
