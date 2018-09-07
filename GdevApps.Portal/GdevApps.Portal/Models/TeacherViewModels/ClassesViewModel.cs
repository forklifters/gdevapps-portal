using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Models.TeacherViewModels
{
    public class ClassesViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Id { get; set; }
        public string Description { get; set; }
        public int? StudentsCount { get; set; }
        public int? CourseWorksCount { get; set; }
        public List<ClassSheetsViewModel> ClassroomSheets {get;set;}
    }
}