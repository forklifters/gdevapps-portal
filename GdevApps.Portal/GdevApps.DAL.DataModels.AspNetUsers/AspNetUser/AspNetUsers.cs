using System;
using System.Collections.Generic;
using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;


namespace GdevApps.DAL.DataModels.AspNetUsers.AspNetUser
{
     public partial class AspNetUsers
    {
         public AspNetUsers()
        {
            AspNetUserClaims = new HashSet<AspNetUserClaims>();
            AspNetUserLogins = new HashSet<AspNetUserLogins>();
            AspNetUserRoles = new HashSet<AspNetUserRoles>();
            Folder = new HashSet<Folder>();
            GradeBook = new HashSet<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>();
            ParentAspUser = new HashSet<Parent>();
            ParentCreatedByNavigation = new HashSet<Parent>();
            ParentGradeBook = new HashSet<ParentGradeBook>();
            ParentSharedGradeBook = new HashSet<ParentSharedGradeBook>();
            ParentStudent = new HashSet<ParentStudent>();
            TeacherAspNetUser = new HashSet<Teacher>();
            TeacherCreatedByNavigation = new HashSet<Teacher>();
        }

        public string Id { get; set; }
        public int AccessFailedCount { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string NormalizedEmail { get; set; }
        public string NormalizedUserName { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }

        public ICollection<AspNetUserClaims> AspNetUserClaims { get; set; }
        public ICollection<AspNetUserLogins> AspNetUserLogins { get; set; }
        public ICollection<AspNetUserRoles> AspNetUserRoles { get; set; }
        public ICollection<Folder> Folder { get; set; }
        public ICollection<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook> GradeBook { get; set; }
        public ICollection<Parent> ParentAspUser { get; set; }
        public ICollection<Parent> ParentCreatedByNavigation { get; set; }
        public ICollection<ParentGradeBook> ParentGradeBook { get; set; }
        public ICollection<ParentSharedGradeBook> ParentSharedGradeBook { get; set; }
        public ICollection<ParentStudent> ParentStudent { get; set; }
        public ICollection<Teacher> TeacherAspNetUser { get; set; }
        public ICollection<Teacher> TeacherCreatedByNavigation { get; set; }
    }
}
