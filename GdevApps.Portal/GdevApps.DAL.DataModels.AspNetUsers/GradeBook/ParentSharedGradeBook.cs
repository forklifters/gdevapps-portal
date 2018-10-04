using System;

namespace GdevApps.DAL.DataModels.AspNetUsers.GradeBook
{
   public partial class ParentSharedGradeBook
    {
        public int ParentGradeBookId { get; set; }
        public string TeacherAspId { get; set; }
        public string ParentAspId { get; set; }
        public int FolderId { get; set; }
        public int Id { get; set; }
        public int SharedStatus { get; set; }
        public int ParentId { get; set; }
        public Folder Folder { get; set; }
        public GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent Parent { get; set; }
        public GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook ParentGradeBook { get; set; }
        public SharedStatus SharedStatusNavigation { get; set; }
        public GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUsers TeacherAsp { get; set; }
    }
}
