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
        public void Configure(EntityTypeBuilder<ParentSharedGradeBook> builder)
        {
            builder.HasKey(k => new {k.Id});
        }
    }
}