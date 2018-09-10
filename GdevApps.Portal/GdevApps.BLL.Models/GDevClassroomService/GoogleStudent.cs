using System.Collections.Generic;

namespace GdevApps.BLL.Models.GDevClassroomService
{
    public class GoogleStudent
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string Email { get; set; }

        public bool IsInClassroom {get; set;}

        public List<string> PrentEmails { get; set; }
        public string ClassId { get; set; }
    }
}