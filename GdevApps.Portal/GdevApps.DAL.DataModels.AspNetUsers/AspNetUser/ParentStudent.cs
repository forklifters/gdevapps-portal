using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataModels.AspNetUsers.AspNetUser
{
    public partial class ParentStudent
    {
        public string ParentAspId { get; set; }
        public string StudentEmail { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int GradeBookId { get; set; }
        public GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook GradeBook { get; set; }
        public Parent Parent { get; set; }
        public AspNetUsers ParentAsp { get; set; }
    }
}
