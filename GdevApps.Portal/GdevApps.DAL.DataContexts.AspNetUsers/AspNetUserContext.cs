using Microsoft.EntityFrameworkCore;
using GdevApps.DAL.DataModels.AspNetUsers;
using GdevApps.DAL.DataContexts.AspNetUsers.Config;

namespace GdevApps.DAL.DataContexts.AspNetUsers
{
    public class AspNetUserContext : DbContext
    {
        public AspNetUserContext(DbContextOptions<AspNetUserContext> options) :
            base(options)
        { }

        public AspNetUserContext() { }

        public virtual DbSet<DataModels.AspNetUsers.AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    builder.ApplyConfiguration(new AspNetRoleClaimsConfig());
        //    builder.ApplyConfiguration(new AspNetRolesConfig());
        //    builder.ApplyConfiguration(new AspNetUserClaimsConfig());
        //    builder.ApplyConfiguration(new AspNetUserConfig());
        //    builder.ApplyConfiguration(new AspNetUserLoginsConfig());
        //    builder.ApplyConfiguration(new AspNetUserRolesConfig());
        //    builder.ApplyConfiguration(new AspNetUserTokensConfig());
        //}
    }
}
