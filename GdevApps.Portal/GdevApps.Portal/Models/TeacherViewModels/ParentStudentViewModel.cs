using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Models.TeacherViewModels
{
    public class ParentStudentViewModel
    {
        public string Email { get; set; }
        public bool HasAccount {get; set;}
        public string StudentEmail{get;set;}
        public string ParentGradebookName {get;set;}
        public string ParentGradebookUniqueId {get;set;}
        public string MainGradeBookName{get;set;}
        public string MainGradeBookNameUniqueId {get;set;}
        public string MainGradeBookLink {get;set;}
        public string ParentGradebookLink {get;set;}
    }
}