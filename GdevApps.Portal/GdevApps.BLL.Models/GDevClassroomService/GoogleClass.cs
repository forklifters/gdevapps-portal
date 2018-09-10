using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevClassroomService
{
    public class GoogleClass
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public int? StudentsCount { get; set; }
        public int? CourseWorksCount { get; set; }
        public List<GoogleClassSheets> ClassroomSheets {get;set;}
    }
}