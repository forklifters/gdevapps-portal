using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevClassroomService
{
    public class GradebookCourseWork
    {
        public virtual string Title { get; set; }
        public virtual string MaxPoints { get; set; }
        public virtual string DueDate { get; set; }
        public virtual string CreationTime { get; set; }
        public virtual string Weight { get; set; }
        public virtual string ClassId { get; set; }
        public virtual string IdInGradeBook { get; set; }
        public virtual string IdInClassroom { get; set; }
        public virtual string GradebookId { get; set; }
        public string Category { get; set; }
        public string Term { get; set; }
    }
}