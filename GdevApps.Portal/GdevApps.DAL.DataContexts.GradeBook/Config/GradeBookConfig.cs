using GdevApps.DAL.DataModels.GradeBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.Gradebook.Config
{
     public class GradeBookConfig : IEntityTypeConfiguration<GdevApps.DAL.DataModels.GradeBook.GradeBook>
    {
        public void Configure(EntityTypeBuilder<GdevApps.DAL.DataModels.GradeBook.GradeBook> builder)
        {
            builder.HasKey(k => new {k.Id});
        }
    }
}