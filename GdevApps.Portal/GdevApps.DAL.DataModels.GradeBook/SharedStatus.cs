using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataModels.GradeBook
{
     public partial class SharedStatus
    {
        public SharedStatus()
        {
            ParentSharedGradeBook = new HashSet<ParentSharedGradeBook>();
        }

        public int Id { get; set; }
        public string Status { get; set; }

        public ICollection<ParentSharedGradeBook> ParentSharedGradeBook { get; set; }
    }
}
