using GdevApps.DAL.DataModels.AspNetUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config
{
    public class AspNetUserRolesConfig : IEntityTypeConfiguration<AspNetUserRoles>
    {
        public void Configure(EntityTypeBuilder<AspNetUserRoles> builder)
        {
            throw new NotImplementedException();
        }
    }
}
