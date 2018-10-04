using System;
using System.Collections.Generic;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;

namespace GdevApps.DAL.DataModels.AspNetUsers.GradeBook
{
     public partial class GradeBook
    {
        public GradeBook()
        {
            ParentGradeBook = new HashSet<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>();
            ParentStudent = new HashSet<ParentStudent>();
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
        public ICollection<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook> ParentGradeBook { get; set; }
        public ICollection<ParentStudent> ParentStudent { get; set; }
    }
}
