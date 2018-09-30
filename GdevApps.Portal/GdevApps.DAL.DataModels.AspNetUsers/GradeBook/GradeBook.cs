using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataModels.AspNetUsers.GradeBook
{
     public partial class GradeBook
    {
        public GradeBook()
        {
            ParentGradeBook = new HashSet<ParentGradeBook>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string GoogleUniqueId { get; set; }
        public string Link { get; set; }
        public string ClassroomId { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUsers CreatedByNavigation { get; set; }
        public ICollection<ParentGradeBook> ParentGradeBook { get; set; }
    }
}
