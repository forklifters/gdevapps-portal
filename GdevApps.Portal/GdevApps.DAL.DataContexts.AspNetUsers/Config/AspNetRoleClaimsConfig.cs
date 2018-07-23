using GdevApps.DAL.DataModels.AspNetUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config
{
    public class AspNetRoleClaimsConfig : IEntityTypeConfiguration<AspNetRoleClaims>
    {
        public void Configure(EntityTypeBuilder<AspNetRoleClaims> builder)
        {
            throw new NotImplementedException();
        }
    }
}
