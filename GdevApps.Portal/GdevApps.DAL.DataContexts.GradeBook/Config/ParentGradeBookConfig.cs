using GdevApps.DAL.DataModels.GradeBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.Gradebook.Config
{
     public class ParentGradeBookConfig : IEntityTypeConfiguration<ParentGradeBook>
    {
        public void Configure(EntityTypeBuilder<ParentGradeBook> builder)
        {
            builder.HasKey(k => new {k.Id});
        }
    }
}