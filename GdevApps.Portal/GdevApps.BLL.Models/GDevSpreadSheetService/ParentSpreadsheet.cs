using System;
using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevSpreadSheetService
{
    public class ParentSpreadsheet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GoogleUniqueId { get; set; }
        public string Link { get; set; }
        public string ClassroomName { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int MainGradeBookId { get; set; }
        public string MainGradeBookName { get; set; }
    }
}