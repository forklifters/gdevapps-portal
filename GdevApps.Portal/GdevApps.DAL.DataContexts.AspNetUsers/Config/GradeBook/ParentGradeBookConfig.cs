using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
     public class ParentGradeBookConfig : IEntityTypeConfiguration<ParentGradeBook>
    {
        public void Configure(EntityTypeBuilder<ParentGradeBook> entity)
        {
             entity.HasIndex(e => e.CreatedBy)
                    .HasName("aspnetuser_parentGradebook_id_idx");
                entity.HasIndex(e => e.GoogleUniqueId)
                    .HasName("google_unique_id_UNIQUE")
                    .IsUnique();
                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();
                entity.HasIndex(e => e.MainGradeBookId)
                    .HasName("gradebook_parentgradebook_id_idx");
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");
                entity.Property(e => e.ClassroomName)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.GoogleUniqueId)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.IsDeleted).HasColumnType("bit(1)");
                entity.Property(e => e.Link)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.MainGradeBookId).HasColumnType("int(11)");
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.ParentGradeBook)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("aspnetuser_parentGradebook_id");
                entity.HasOne(d => d.MainGradeBook)
                    .WithMany(p => p.ParentGradeBook)
                    .HasForeignKey(d => d.MainGradeBookId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("gradebook_parentgradebook_id");
        }
    }
}