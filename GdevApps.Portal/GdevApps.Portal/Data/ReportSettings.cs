using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GdevApps.Portal.Data
{
    public class ReportSettings
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CoursePeriod { get; set; }
        public string TeacherName { get; set; }
        public double CourseAverage { get; set; }
        public double CourseMedian { get; set; }
    }
}


