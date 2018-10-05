using System;
using System.Collections.Generic;
using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;

namespace GdevApps.DAL.DataModels.AspNetUsers.AspNetUser
{
   public partial class Teacher
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string AspNetUserId { get; set; }

        public AspNetUsers AspNetUser { get; set; }
    }
}
