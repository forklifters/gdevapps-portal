using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
     public class FolderTypeConfig : IEntityTypeConfiguration<FolderType>
    {
        public void Configure(EntityTypeBuilder<FolderType> entity)
        {
             entity.HasIndex(e => e.FolderType1)
                    .HasName("FolderType_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.FolderType1)
                    .IsRequired()
                    .HasColumnName("FolderType")
                    .HasMaxLength(150);
        }
    }
}