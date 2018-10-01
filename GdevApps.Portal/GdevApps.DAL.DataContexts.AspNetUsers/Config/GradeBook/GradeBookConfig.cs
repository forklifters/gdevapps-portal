using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
     public class GradeBookConfig : IEntityTypeConfiguration<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>
    {
        public void Configure(EntityTypeBuilder<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook> entity)
        {
           entity.HasIndex(e => e.ClassroomId)
                    .HasName("classroom_id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.CreatedBy)
                    .HasName("aspnet_user_id_idx");

                entity.HasIndex(e => e.GoogleUniqueId)
                    .HasName("google_unique_id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.ClassroomId)
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

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.GradeBook)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("aspnet_user_id");
        }
    }
}