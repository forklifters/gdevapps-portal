using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
     public class ParentConfig : IEntityTypeConfiguration<Parent>
    {
        public void Configure(EntityTypeBuilder<Parent> entity)
        {
            entity.HasIndex(e => e.AspUserId)
                   .HasName("aspnetuser_parent_id_idx");
            entity.HasIndex(e => e.Email)
                .HasName("Email_UNIQUE")
                .IsUnique();
            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.AspUserId).HasMaxLength(256);
            entity.Property(e => e.Avatar)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(256);
            entity.HasOne(d => d.AspUser)
                .WithMany(p => p.Parent)
                .HasForeignKey(d => d.AspUserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("aspnetuser_parent_id");
        }
    }
}