using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GdevApps.DAL.DataContexts.AspNetUsers;
using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;
using GdevApps.DAL.Repositories.BaseRepository;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GdevApps.DAL.Repositories.GradeBookRepository
{
    public class GradeBookRepository :  EntityFrameworkRepository<AspNetUserContext>,
        IGradeBookRepository 
    {

         public GradeBookRepository(AspNetUserContext context) :
            base(context)
        {
            context.ParentStudent.Include(p => p.Parent).LoadAsync();
            context.ParentStudent.Include(p => p.GradeBook).LoadAsync();
        }

        public GradeBookRepository() :
            base(new AspNetUserContext())
        {
        }

        public Task<List<GradeBook>> GetMainGradebooksByParentEmailAndStudentEmailAsync(string parentEmail, string studentEmail)
        {
            if(string.IsNullOrWhiteSpace(parentEmail))
            {
                throw new ArgumentNullException();
            }
            var gradebooks = from gb in context.GradeBook
                                join ps in context.ParentStudent on gb.Id equals ps.GradeBookId
                                join p in context.Parent on ps.ParentId equals p.Id
                                where p.Email == parentEmail && ps.StudentEmail == studentEmail
                                select gb;

            return gradebooks.ToListAsync();
        }
    }
}
