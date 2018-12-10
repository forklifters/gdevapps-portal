using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.BLL.Models.AspNetUsers
{
    public partial class Teacher
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string AspNetUserId { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByEmail { get; set; }
    }
}
