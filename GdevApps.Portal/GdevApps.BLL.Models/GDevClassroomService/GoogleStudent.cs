using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevClassroomService
{
    public class GoogleStudent
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string ParentEmail { get; set; }
        public string Comment { get; set; }
        public string Photo { get; set; }
        public List<string> ParentEmails { get; set; }
        public string ClassId { get; set; }
        public string FinalGrade{get;set;}
        public string GradebookId { get; set; }
        public bool IsInClassroom {get; set;}
    }
}