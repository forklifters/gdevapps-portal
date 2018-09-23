using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Models.TeacherViewModels
{
    public class StudentsViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Id { get; set; }

        [Required]
        public string Email { get; set; }

        public bool IsInClassroom {get; set;}

        public List<string> ParentEmails { get; set; }
        public string ClassId { get; set; }
    }
}