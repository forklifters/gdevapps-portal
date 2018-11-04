using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevSpreadSheetService
{
    public class GradebookStudentSubmission
    {
      public string ClassId { get; set; }
      public string Grade {get;set;}
      public double Percent {get;set;}
      public string CourseWorkId { get; set; }
      public string StudentName { get; set; }
      public string StudentId { get; set; }
      public string Note { get; set; }
      public string StudentClassroomId { get; set; }
    }
}