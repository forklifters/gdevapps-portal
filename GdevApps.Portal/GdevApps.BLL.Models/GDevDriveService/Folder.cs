using System;
using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevDriveService
{
    public class Folder
    {
         public int Id { get; set; }
        public string FolderName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int FolderType { get; set; }
        public bool IsDeleted { get; set; }
        public string GoogleFileId { get; set; }
        public int? PrentFolderId { get; set; }
    }
}