using GdevApps.DAL.DataModels.AspNetUsers;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
    public class AspNetUserLoginsConfig : IEntityTypeConfiguration<AspNetUserLogins>
    {
        public void Configure(EntityTypeBuilder<AspNetUserLogins> entity)
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.LoginProvider).HasMaxLength(255);

            entity.Property(e => e.ProviderKey).HasMaxLength(255);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.User)
                .WithMany(p => p.AspNetUserLogins)
                .HasForeignKey(d => d.UserId);
        }
    }
}
