using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevSpreadSheetService
{
    public sealed class GradebookReportInfo : StudentReport
    {
        private readonly GradebookType _type;

        public GradebookReportInfo(GradebookType type)
        {
            _type = type;
        }

        public override double TotalPoints { get; set; }
        public override double TotalMark { get; set; }
        public override string StudentMark { get; set; }
        public override string Percent {get; set;}
        public override GradebookType Type => _type;
        public override List<GradebookReportSubmission> Submissions {get; set;}
    }
}