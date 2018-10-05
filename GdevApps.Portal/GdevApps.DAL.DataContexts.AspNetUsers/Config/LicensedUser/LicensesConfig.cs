using GdevApps.DAL.DataModels.AspNetUsers.LicensedUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GdevApps.DAL.DataContexts.AspNetUsers.Config.AspNetUser
{
     public class LicensesConfig : IEntityTypeConfiguration<Licenses>
    {
        public void Configure(EntityTypeBuilder<Licenses> entity)
        {
             
        }
    }
}