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
        public void Configure(EntityTypeBuilder<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook> builder)
        {
            builder.HasKey(k => new {k.Id});
        }
    }
}