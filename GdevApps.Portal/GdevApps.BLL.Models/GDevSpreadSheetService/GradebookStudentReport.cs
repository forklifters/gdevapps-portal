using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevSpreadSheetService
{
    public class GradebookStudentReport<T> where T: class 
    {
        public List<T> ReportInfos {get; set;}
        public string FinalGrade { get; set; }
        public GradebookStudent Student { get; set; }
    }
}