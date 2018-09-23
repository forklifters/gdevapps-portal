using System.Collections.Generic;

namespace GdevApps.BLL.Models
{
    public class BoolResult
    {
        public BoolResult(bool result)
        {
            Result = result;
        }
        public bool Result { get; set; }
    }
}