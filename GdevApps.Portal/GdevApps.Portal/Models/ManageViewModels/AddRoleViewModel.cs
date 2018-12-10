using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GdevApps.BLL.Models.AspNetUsers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GdevApps.Portal.Models.ManageViewModels
{
    public class AddRoleViewModel
    {
        public AddRoleViewModel(string roleName)
        {
            RoleName = roleName;
        }

        public AddRoleViewModel()
        {
        }

        [Required(ErrorMessage="User email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage="User name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage="User role is required")]
        public string RoleName { get; set; }
    }
}
