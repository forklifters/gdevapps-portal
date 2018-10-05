using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Models.AccountViewModels
{
    public class AccountLoginInfo
    {
        public bool isParent { get; set; }
        public bool isTeacher { get; set; }
    }
}
