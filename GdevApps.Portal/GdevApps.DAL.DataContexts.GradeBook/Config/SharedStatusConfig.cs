using GdevApps.DAL.DataModels.GradeBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.Gradebook.Config
{
     public class SharedStatusConfig : IEntityTypeConfiguration<SharedStatus>
    {
        public void Configure(EntityTypeBuilder<SharedStatus> builder)
        {
            builder.HasKey(k => new {k.Id});
        }
    }
}