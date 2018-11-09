 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace GdevApps.Portal.Data
{
    public class StudentSubmission 
    {
        private string _dueDate;
        public string Grade { get; set; }
        public double Percent { get; set; }
        public string CourseWorkId { get; set; }
        public string StudentId { get; set; }
        public string Note { get; set; }
        public virtual string Title { get; set; }
        public virtual string MaxPoints { get; set; }
        public virtual string DueDate
        {
            get
            {
                return _dueDate;
            }
             set
             {
                DateTime dueDate;
                if(DateTime.TryParse(value, out dueDate))
                {
                    _dueDate = dueDate.ToString("yyyy-MM-dd");
                }
             }
        }
        public virtual string CreationTime { get; set; }
    }
}

 
 
