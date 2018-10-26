using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GdevApps.Portal.Models.TeacherViewModels
{
    public class ParentStudentsViewModel
    {
        public SelectList Students { get; set; }
    }
}