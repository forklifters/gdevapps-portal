using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevSpreadSheetService
{
    public sealed class GradebookStudentCategoryInfo : StudentReport
    {
        private readonly GradebookType _type;

        public GradebookStudentCategoryInfo(GradebookType type)
        {
            _type = type;
        }

        public override string Category { get; set; }
        public override double AverageCatGrade { get; set; }
        public override double AverageStudentGrade { get; set; }
        public override double TotalWeight { get; set; }
        public override string StudentMark { get; set; }
        public override string Percent { get; set; }
        public override GradebookType Type => _type;
        public override List<GradebookReportSubmission> Submissions { get; set; }
    }
}