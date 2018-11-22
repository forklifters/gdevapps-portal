using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
     public class TeacherConfig : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> entity)
        {
            entity.HasIndex(e => e.AspNetUserId)
                    .HasName("aspnetuser_teacher_id_idx");
                entity.HasIndex(e => e.CreatedBy)
                    .HasName("createdBy_aspnetuser_id_idx");
                entity.Property(e => e.Id).HasColumnType("int(11)");
                entity.Property(e => e.AspNetUserId).HasMaxLength(255);
                entity.Property(e => e.Avatar)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.CreatedBy).HasMaxLength(255);
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.HasOne(d => d.AspNetUser)
                    .WithMany(p => p.TeacherAspNetUser)
                    .HasForeignKey(d => d.AspNetUserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("aspnetuser_teacher_id");
                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.TeacherCreatedByNavigation)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("createdBy_aspuserId");
        }
    }
}