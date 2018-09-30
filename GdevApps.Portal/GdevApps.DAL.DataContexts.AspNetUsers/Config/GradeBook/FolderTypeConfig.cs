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
        public void Configure(EntityTypeBuilder<FolderType> builder)
        {
            builder.HasKey(k => new {k.Id});
        }
    }
}