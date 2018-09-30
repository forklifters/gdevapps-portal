using GdevApps.DAL.DataModels.AspNetUsers;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
    public class AspNetRoleClaimsConfig : IEntityTypeConfiguration<AspNetRoleClaims>
    {
        public void Configure(EntityTypeBuilder<AspNetRoleClaims> entity)
        {
            entity.HasIndex(e => e.RoleId);

            entity.Property(e => e.Id).HasColumnType("int(11)");

            entity.Property(e => e.RoleId)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.Role)
                .WithMany(p => p.AspNetRoleClaims)
                .HasForeignKey(d => d.RoleId);
        }
    }
}
