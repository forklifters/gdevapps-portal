using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevSpreadSheetService
{
    public class GradebookStudent
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
        public string Photo { get; set; }
        public List<GradebookParent> Parents { get; set; }
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string FinalGrade{get;set;}
        public string GradebookId { get; set; }
        public IEnumerable<GradebookStudentSubmission> Submissions{get;set;}
        public IEnumerable<GradebookCourseWork> CourseWorks{get;set;}
    }
}