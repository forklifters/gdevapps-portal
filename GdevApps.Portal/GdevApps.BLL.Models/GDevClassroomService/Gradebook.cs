using System;

namespace GdevApps.BLL.Models.GDevClassroomService
{
    public class GradeBook
    {
         public int Id { get; set; }
        public string Name { get; set; }
        public string GoogleUniqueId { get; set; }
        public string Link { get; set; }
        public string ClassroomId { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}