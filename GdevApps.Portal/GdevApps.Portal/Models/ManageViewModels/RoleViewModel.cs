using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Models.ManageViewModels
{
    public class RoleViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string UserName {get;set;}
        public string Avatar { get; set; }
        public string AspUserId { get; set; }
        public string CreatedByEmail { get; set; }
        public string CreatedById { get; set; }
        public List<string> StudentEmails {get;set;} = new List<string>();
    }
}
