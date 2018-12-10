using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GdevApps.Portal.Models.ManageViewModels
{
    public class GeneralAddRoleViewModel : AddRoleViewModel
    {
        public SelectList Roles { get; set; }
    }
}
