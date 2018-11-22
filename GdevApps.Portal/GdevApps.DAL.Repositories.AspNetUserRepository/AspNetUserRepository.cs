using GdevApps.DAL.DataContexts.AspNetUsers;
using GdevApps.DAL.Repositories.BaseRepository;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;

namespace GdevApps.DAL.Repositories.AspNetUserRepository
{
    public class AspNetUserRepository :
        EntityFrameworkRepository<AspNetUserContext>,
        IAspNetUserRepository
    {
        public AspNetUserRepository(AspNetUserContext context) :
            base(context)
        {
            context.Parent.Include(p => p.AspUser).LoadAsync();
            context.Parent.Include(p => p.CreatedBy).LoadAsync();
            context.Parent.Include(p => p.ParentSharedGradeBook).LoadAsync();
            context.Parent.Include(p => p.ParentStudent).LoadAsync();
            context.ParentSharedGradeBook.Include(p => p.ParentGradeBook).LoadAsync();
            context.ParentStudent.Include(ps => ps.GradeBook).LoadAsync();
        }

        public AspNetUserRepository() :
            base(new AspNetUserContext())
        {
        }

        //TODO: Replace this method with a normal one
        public async Task<List<Parent>> GetParentsByCreatorIdTempAsync(string creatorId)
        {
            if(string.IsNullOrWhiteSpace(creatorId))
            {
                throw new ArgumentNullException();
            }
            var parents = from p in context.Parent
                             join ps in context.ParentSharedGradeBook on p.Id equals ps.ParentId
                             join pg in context.ParentGradeBook on ps.ParentGradeBookId equals pg.Id
                             join mg in context.GradeBook on pg.MainGradeBookId equals mg.Id
                             select p;

            return await parents.ToListAsync();
        }
    }
}
