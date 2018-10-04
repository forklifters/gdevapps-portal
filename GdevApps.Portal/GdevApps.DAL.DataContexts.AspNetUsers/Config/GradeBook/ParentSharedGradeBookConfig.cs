using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
    public class ParentSharedGradeBookConfig : IEntityTypeConfiguration<ParentSharedGradeBook>
    {
        public void Configure(EntityTypeBuilder<ParentSharedGradeBook> entity)
        {
            entity.HasIndex(e => e.FolderId)
                     .HasName("folder_shared_id_idx");
            entity.HasIndex(e => e.Id)
                .HasName("id_UNIQUE")
                .IsUnique();
            entity.HasIndex(e => e.ParentAspId)
                .HasName("parent_shared_id_idx");
            entity.HasIndex(e => e.ParentGradeBookId)
                .HasName("parentgradebook_shared_id_idx");
            entity.HasIndex(e => e.ParentId)
                .HasName("parentshared_parent_id_idx");
            entity.HasIndex(e => e.SharedStatus)
                .HasName("status_shared_id_idx");
            entity.HasIndex(e => e.TeacherAspId)
                .HasName("aspnet_user_shared_id_idx");
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("int(11)");
            entity.Property(e => e.FolderId).HasColumnType("int(11)");
            entity.Property(e => e.ParentAspId).HasMaxLength(255);
            entity.Property(e => e.ParentGradeBookId).HasColumnType("int(11)");
            entity.Property(e => e.ParentId).HasColumnType("int(11)");
            entity.Property(e => e.SharedStatus).HasColumnType("int(11)");
            entity.Property(e => e.TeacherAspId)
                .IsRequired()
                .HasMaxLength(256);
            entity.HasOne(d => d.Folder)
                .WithMany(p => p.ParentSharedGradeBook)
                .HasForeignKey(d => d.FolderId)
                .HasConstraintName("folder_shared_id");
            entity.HasOne(d => d.ParentGradeBook)
                .WithMany(p => p.ParentSharedGradeBook)
                .HasForeignKey(d => d.ParentGradeBookId)
                .HasConstraintName("parentgradebook_shared_id");
            entity.HasOne(d => d.Parent)
                .WithMany(p => p.ParentSharedGradeBook)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("parentshared_parent_id");
            entity.HasOne(d => d.SharedStatusNavigation)
                .WithMany(p => p.ParentSharedGradeBook)
                .HasForeignKey(d => d.SharedStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("status_shared_id");
            entity.HasOne(d => d.TeacherAsp)
                .WithMany(p => p.ParentSharedGradeBook)
                .HasForeignKey(d => d.TeacherAspId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("teacherasp_aspuser_id");
        }
    }
}