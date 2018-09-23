using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevClassroomService
{
    public class GradebookCourseWork
    {
        public virtual string Title { get; set; }
        public virtual double? MaxPoints { get; set; }
        public virtual string Id { get; set; }
        public virtual string DueDate { get; set; }
        public virtual string ClassId { get; set; }
        public virtual string GradebookId { get; set; }
    }
}