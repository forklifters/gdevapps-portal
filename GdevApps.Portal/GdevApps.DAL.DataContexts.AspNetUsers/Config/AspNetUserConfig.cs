using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config
{
    public class AspNetUserConfig : IEntityTypeConfiguration<DataModels.AspNetUsers.AspNetUsers>
    {
        public void Configure(EntityTypeBuilder<DataModels.AspNetUsers.AspNetUsers> builder)
        {
            builder.HasKey(k => new {k.Id});
        }
    }
}
