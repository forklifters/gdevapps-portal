using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevSpreadSheetService
{
    public abstract class StudentReport
    {
        public virtual string Category { get; set; }
        public virtual double AverageCatGrade { get; set; }
        public virtual double AverageStudentGrade { get; set; }
        public virtual double TotalWeight { get; set; }
        public virtual string StudentMark { get; set; }
        public virtual string Percent {get; set;}
        public virtual double TotalPoints { get; set; }
        public virtual double TotalMark { get; set; }
        public abstract GradebookType Type { get; }
        public bool IsGraded { get; set; }
        public virtual List<GradebookReportSubmission> Submissions {get; set;}
    }
}