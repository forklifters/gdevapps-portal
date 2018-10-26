using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
    public class ParentStudentConfig : IEntityTypeConfiguration<ParentStudent>
    {
        public void Configure(EntityTypeBuilder<ParentStudent> entity)
        {
            entity.HasIndex(e => e.GradeBookId)
                   .HasName("paarentstudent_gradebook_id_idx");
            entity.HasIndex(e => e.ParentAspId)
                .HasName("parent_student_id_idx");
            entity.HasIndex(e => e.ParentId)
                .HasName("parentstudent_parent_id_idx");
            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.GradeBookId).HasColumnType("int(11)");
            entity.Property(e => e.ParentAspId).HasMaxLength(256);
            entity.Property(e => e.ParentId).HasColumnType("int(11)");
            entity.Property(e => e.StudentEmail)
                .IsRequired()
                .HasMaxLength(45);
            entity.HasOne(d => d.GradeBook)
                .WithMany(p => p.ParentStudent)
                .HasForeignKey(d => d.GradeBookId)
                .HasConstraintName("paarentstudent_gradebook_id");
            entity.HasOne(d => d.ParentAsp)
                .WithMany(p => p.ParentStudent)
                .HasForeignKey(d => d.ParentAspId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("parentstudentasp_aspuser_id");
            entity.HasOne(d => d.Parent)
                .WithMany(p => p.ParentStudent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("parentstudent_parent_id");
        }
    }
}