using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataContexts.GradebookLicense.Model
{
    public partial class Logs
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string LogValue { get; set; }
        public string LogType { get; set; }
    }
}
