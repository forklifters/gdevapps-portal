using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GdevApps.BLL.Models.AspNetUsers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GdevApps.Portal.Models.ManageViewModels
{
    public class AddUserViewModel
    {
        public AddUserViewModel()
        {
            var roleList = new List<object>()
            {
                new 
                {
                    Id = -1,
                    Value = ""
                },
                new 
                {
                    Id = UserRoles.Admin,
                    Value = UserRoles.Admin
                },
                new 
                {
                    Id = UserRoles.Teacher,
                    Value = UserRoles.Teacher
                }
            };
            Roles = new SelectList(roleList, "Id", "Value");
        }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string UserEmail { get; set; }

        [Required(ErrorMessage="User name is required")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage="Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage="User role is required")]
        public string UserRole {get;set;}

        [Display(Name = "Select role")]
        public SelectList Roles{get;set;}
    }
}
