using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevSpreadSheetService
{
    public class GradebookSettings
    {
        public string MainGradeBookName { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CoursePeriod { get; set; }
        public string TeacherName { get; set; }
        public string SchoolName { get; set; }
        public string SchoolPhone { get; set; }
        public string Rounding { get; set; }
        public int Decimal { get; set; }
        public bool ShowCourseAverage { get; set; }
        public bool ShowCourseMedian { get; set; }
        public bool SendEmailsAsNoReply { get; set; }
        public string StudentReportSortBy { get; set; }
        public string ReportSchoolNameColor {get; set;}
        public string ReportHeadingColor {get; set;}
        public string ReportAlternatingColorFirst {get; set;}
        public string ReportAlternatingColorSecond {get; set;}
        public int Terms { get; set; }
        public double CourseAverage { get; set; }
        public double CourseMedian { get; set; }
        public List<LetterGrade> LetterGrades {get;set;}
    }
}