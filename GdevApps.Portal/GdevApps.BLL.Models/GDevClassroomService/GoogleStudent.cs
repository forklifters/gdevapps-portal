using System.Collections.Generic;
using GdevApps.BLL.Models.GDevSpreadSheetService;

namespace GdevApps.BLL.Models.GDevClassroomService
{
    public class GoogleStudent
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
        public bool IsInClassroom {get; set;}
    }
}