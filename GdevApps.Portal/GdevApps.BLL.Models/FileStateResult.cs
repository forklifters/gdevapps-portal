using System.Collections.Generic;

namespace GdevApps.BLL.Models
{
    public class FileStateResult
    {
        public FileStateResult(FileState result)
        {
            Result = result;
        }

        public FileState Result { get; set; }
    }
}