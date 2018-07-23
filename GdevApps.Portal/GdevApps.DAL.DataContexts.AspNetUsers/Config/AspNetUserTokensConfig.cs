using GdevApps.DAL.DataModels.AspNetUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config
{
    public class AspNetUserTokensConfig : IEntityTypeConfiguration<AspNetUserTokens>
    {
        public void Configure(EntityTypeBuilder<AspNetUserTokens> builder)
        {
            throw new NotImplementedException();
        }
    }
}
