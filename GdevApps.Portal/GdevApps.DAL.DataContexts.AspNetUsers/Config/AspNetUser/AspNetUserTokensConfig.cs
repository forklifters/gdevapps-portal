using GdevApps.DAL.DataModels.AspNetUsers;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
    public class AspNetUserTokensConfig : IEntityTypeConfiguration<AspNetUserTokens>
    {
        public void Configure(EntityTypeBuilder<AspNetUserTokens> entity)
        {
            entity.ToTable("AspNetUserTokens", "gradebook_license");

            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.UserId).HasMaxLength(255);

            entity.Property(e => e.LoginProvider).HasMaxLength(255);

            entity.Property(e => e.Name).HasMaxLength(255);
        }
    }
}
