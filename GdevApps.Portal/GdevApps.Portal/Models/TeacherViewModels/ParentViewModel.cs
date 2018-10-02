using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Models.TeacherViewModels
{
    public class ParentViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        public bool HasAccount {get; set;}
    }
}