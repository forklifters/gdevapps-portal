using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevSpreadSheetService
{
    public class GradebookStudentReport<T> where T: class 
    {
        public List<T> ReportInfos {get; set;}
        public double FinalGrade { get; set; }
        public string FinalGradeLetter { get; set; }
        public GradebookStudent Student { get; set; }
    }
}