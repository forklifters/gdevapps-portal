using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataModels.AspNetUsers.GradeBook
{
     public partial class Folder
    {
        public Folder()
        {
            ParentSharedGradeBook = new HashSet<ParentSharedGradeBook>();
        }

        public int Id { get; set; }
        public string FolderName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int FolderType { get; set; }
        public bool IsDeleted { get; set; }
        public GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUsers CreatedByNavigation { get; set; }
        public FolderType FolderTypeNavigation { get; set; }
        public ICollection<ParentSharedGradeBook> ParentSharedGradeBook { get; set; }
    }
}
