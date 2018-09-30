using GdevApps.DAL.DataModels.AspNetUsers;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
    public class AspNetUserClaimsConfig : IEntityTypeConfiguration<AspNetUserClaims>
    {
        public void Configure(EntityTypeBuilder<AspNetUserClaims> entity)
        {
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.Id).HasColumnType("int(11)");

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.User)
                .WithMany(p => p.AspNetUserClaims)
                .HasForeignKey(d => d.UserId);
        }
    }
}
