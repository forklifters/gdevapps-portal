using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace GdevApps.DAL.DataModels.AspNetUsers.AspNetUser
{
    public class ApplicationUser : IdentityUser
    {
        public string Avatar { get; set;}
    }
}
