using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GdevApps.Portal.Data;
using GdevApps.Portal.Models.SharedViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GdevApps.Portal.Models.SharedViewModels
{
    public class StudentReportsViewModel
    {
        public SelectList Students { get; set; }
        public List<ReportsViewModel> StudentReports { get; set; }
        public string StudentName { get; set; }
        public string StudentGrade { get; set; }
        public double FinalGrade { get; set; }
        public string LetterGrade { get; set; }
        public ReportSettings ReportSettings { get; set; }
        public string ReportGenerationDate
        {
            get
            {
                return DateTime.UtcNow.ToString("yyyy-MM-dd");
            }
        }
    }
}