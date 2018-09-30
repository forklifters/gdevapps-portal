using Microsoft.EntityFrameworkCore;
using GdevApps.DAL.DataModels.AspNetUsers;
using GdevApps.DAL.DataContexts.AspNetUsers.Config;
using GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser;
using GdevApps.DAL.DataContexts.AspNetUsers.Config.GradeBook;

namespace GdevApps.DAL.DataContexts.AspNetUsers
{
    public class AspNetUserContext : DbContext
    {
        public AspNetUserContext(DbContextOptions<AspNetUserContext> options) :
            base(options)
        { }

        public AspNetUserContext() { }

        public virtual DbSet<DataModels.AspNetUsers.AspNetUser.AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.AspNetUser.AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.AspNetUser.AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.AspNetUser.AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.AspNetUser.AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.AspNetUser.AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.AspNetUser.AspNetUserTokens> AspNetUserTokens { get; set; }

         public virtual DbSet<DataModels.AspNetUsers.GradeBook.Folder> Folder { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.GradeBook.FolderType> FolderType { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.GradeBook.GradeBook> GradeBook { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.GradeBook.ParentGradeBook> ParentGradeBook { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook> ParentSharedGradeBook { get; set; }
        public virtual DbSet<DataModels.AspNetUsers.GradeBook.SharedStatus> SharedStatus { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
           builder.ApplyConfiguration(new AspNetRoleClaimsConfig());
           builder.ApplyConfiguration(new AspNetRolesConfig());
           builder.ApplyConfiguration(new AspNetUserClaimsConfig());
           builder.ApplyConfiguration(new AspNetUserConfig());
           builder.ApplyConfiguration(new AspNetUserLoginsConfig());
           builder.ApplyConfiguration(new AspNetUserRolesConfig());
           builder.ApplyConfiguration(new AspNetUserTokensConfig());

            builder.ApplyConfiguration(new FolderConfig());
           builder.ApplyConfiguration(new FolderTypeConfig());
           builder.ApplyConfiguration(new GradeBookConfig());
           builder.ApplyConfiguration(new ParentGradeBookConfig());
           builder.ApplyConfiguration(new ParentSharedGradeBookConfig());
           builder.ApplyConfiguration(new SharedStatusConfig());

        }
    }
}
