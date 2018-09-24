using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevClassroomService
{
    public class GradebookStudentSubmission
    {
      public string ClassId { get; set; }
      public string Grade {get;set;}
      public string CourseWorkId { get; set; }
      public string StudentId { get; set; }
      public string Email { get; set; }
      public string Note { get; set; }
      public string StudentClassroomId { get; set; }
    }
}