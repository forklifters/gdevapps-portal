using GdevApps.DAL.DataModels.GradeBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.Gradebook.Config
{
     public class FolderTypeConfig : IEntityTypeConfiguration<FolderType>
    {
        public void Configure(EntityTypeBuilder<FolderType> builder)
        {
            builder.HasKey(k => new {k.Id});
        }
    }
}