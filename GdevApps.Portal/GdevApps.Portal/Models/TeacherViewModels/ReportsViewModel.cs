using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GdevApps.Portal.Data;

namespace GdevApps.Portal.Models.TeacherViewModels
{
    public class ReportsViewModel
    {
        public string Category { get; set; }
        public double AverageCatGrade { get; set; }
        public double AverageStudentGrade { get; set; }
        public double TotalWeight { get; set; }
        public string StudentMark { get; set; }
        public string Percent {get; set;}
        public double TotalPoints { get; set; }
        public double TotalMark { get; set; }
        public bool IsGraded { get; set; }
        public List<StudentSubmission> Submissions {get;set;}
    }
}