using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataModels.GradeBook
{
     public partial class FolderType
    {
        public FolderType()
        {
            Folder = new HashSet<Folder>();
        }

        public int Id { get; set; }
        public string FolderType1 { get; set; }

        public ICollection<Folder> Folder { get; set; }
    }
}
