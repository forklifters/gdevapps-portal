using GdevApps.DAL.DataModels.AspNetUsers;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
    public class AspNetRolesConfig : IEntityTypeConfiguration<AspNetRoles>
    {
        public void Configure(EntityTypeBuilder<AspNetRoles> entity)
        {
            entity.HasIndex(e => e.NormalizedName)
                    .HasName("RoleNameIndex");

            entity.Property(e => e.Id).HasMaxLength(255);

            entity.Property(e => e.Name).HasMaxLength(256);

            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        }
    }
}
