using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevSpreadSheetService
{
    public class GradebookReportSubmission
    {
        public string Grade { get; set; }
        public double Percent { get; set; }
        public string CourseWorkId { get; set; }
        public string StudentId { get; set; }
        public string Note { get; set; }
        public virtual string Title { get; set; }
        public virtual string MaxPoints { get; set; }
        public virtual string DueDate { get; set; }
        public virtual string CreationTime { get; set; }
    }
}