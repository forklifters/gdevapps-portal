using System;
using System.Collections.Generic;
using System.Text;
using GdevApps.BLL.Models.GDevSpreadSheetService;

namespace GdevApps.BLL.Models.AspNetUsers
{
    public class ParentModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string AspUserId { get; set; }
        public List<ParentSpreadsheet> ParentSpreadsheets {get;set;}
    }
}
