using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GdevApps.Portal.Data;
using GdevApps.Portal.Models.SharedViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GdevApps.Portal.Models.TeacherViewModels
{
    public class StudentGradebookReportsViewModel: StudentReportsViewModel
    {
        public SelectList Gradebooks { get; set; }
    }
}