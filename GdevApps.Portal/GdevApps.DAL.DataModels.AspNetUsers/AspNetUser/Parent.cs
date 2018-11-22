using System;
using System.Collections.Generic;
using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;

namespace GdevApps.DAL.DataModels.AspNetUsers.AspNetUser
{
    public partial class Parent
    {
       public Parent()
        {
            ParentSharedGradeBook = new HashSet<ParentSharedGradeBook>();
            ParentStudent = new HashSet<ParentStudent>();
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string AspUserId { get; set; }
        public string CreatedBy { get; set; }

        public AspNetUsers AspUser { get; set; }
        public AspNetUsers CreatedByNavigation { get; set; }
        public ICollection<ParentSharedGradeBook> ParentSharedGradeBook { get; set; }
        public ICollection<ParentStudent> ParentStudent { get; set; }
    }
}
