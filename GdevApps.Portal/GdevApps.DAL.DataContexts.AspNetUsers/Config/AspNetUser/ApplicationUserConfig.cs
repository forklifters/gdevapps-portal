using GdevApps.DAL.DataModels.AspNetUsers;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
    public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> entity)
        {
            entity.Property<string>("Id");

            entity.Property<int>("AccessFailedCount");

            entity.Property<string>("ConcurrencyStamp")
                .IsConcurrencyToken();

            entity.Property<string>("Email")
                .HasAnnotation("MaxLength", 256);

            entity.Property<bool>("EmailConfirmed");

            entity.Property<bool>("LockoutEnabled");

            entity.Property<DateTimeOffset?>("LockoutEnd");

            entity.Property<string>("NormalizedEmail")
                .HasAnnotation("MaxLength", 256);

            entity.Property<string>("NormalizedUserName")
                .HasAnnotation("MaxLength", 256);

            entity.Property<string>("PasswordHash");

            entity.Property<string>("PhoneNumber");

            entity.Property<bool>("PhoneNumberConfirmed");

            entity.Property<string>("SecurityStamp");

            entity.Property<bool>("TwoFactorEnabled");

            entity.Property<string>("UserName")
                .HasAnnotation("MaxLength", 256);

            entity.HasKey("Id");

            entity.HasIndex("NormalizedEmail")
                .HasName("EmailIndex");

            entity.HasIndex("NormalizedUserName")
                .IsUnique()
                .HasName("UserNameIndex");

            entity.ToTable("AspNetUsers");
        }
    }
}
