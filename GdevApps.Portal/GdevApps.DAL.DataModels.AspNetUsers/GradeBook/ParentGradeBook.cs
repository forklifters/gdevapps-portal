using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataModels.AspNetUsers.GradeBook
{
     public partial class ParentGradeBook
    {
        public ParentGradeBook()
        {
            ParentSharedGradeBook = new HashSet<ParentSharedGradeBook>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string GoogleUniqueId { get; set; }
        public string Link { get; set; }
        public string ClassroomName { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int MainGradeBookId { get; set; }
        public GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUsers CreatedByNavigation { get; set; }
        public GradeBook MainGradeBook { get; set; }
        public ICollection<ParentSharedGradeBook> ParentSharedGradeBook { get; set; }
    }
}
