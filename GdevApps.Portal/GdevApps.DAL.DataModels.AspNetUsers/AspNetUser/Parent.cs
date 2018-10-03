using System;
using System.Collections.Generic;

namespace GdevApps.DAL.DataModels.AspNetUsers.AspNetUser
{
    public partial class Parent
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string AspUserId { get; set; }

        public GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUsers AspUser { get; set; }
    }
}
