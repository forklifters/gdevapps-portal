using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.GradeBook
{
     public class FolderConfig : IEntityTypeConfiguration<Folder>
    {
        public void Configure(EntityTypeBuilder<Folder> entity)
        {
              entity.HasIndex(e => e.CreatedBy)
                    .HasName("folder_user_id_idx");

                entity.HasIndex(e => e.FolderType)
                    .HasName("folder_type_id_idx");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.PrentFolderId)
                    .HasName("parentfolder_folder_id_idx");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.FolderName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.FolderType).HasColumnType("int(11)");

                entity.Property(e => e.GoogleFileId)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.IsDeleted).HasColumnType("bit(1)");

                entity.Property(e => e.PrentFolderId).HasColumnType("int(11)");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Folder)
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("folder_user_id");

                entity.HasOne(d => d.FolderTypeNavigation)
                    .WithMany(p => p.Folder)
                    .HasForeignKey(d => d.FolderType)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("folder_type_id");

                entity.HasOne(d => d.PrentFolder)
                    .WithMany(p => p.InversePrentFolder)
                    .HasForeignKey(d => d.PrentFolderId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("parentfolder_folder_id");
        }
    }
}