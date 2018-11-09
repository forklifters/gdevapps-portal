using System.Collections.Generic;
using System.Threading.Tasks;
using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;
using GdevApps.DAL.Repositories.BaseRepository;

namespace GdevApps.DAL.Repositories.GradeBookRepository
{
    public interface IGradeBookRepository : IRepository, IReadOnlyRepository
    {
        Task<List<GradeBook>> GetMainGradebooksByParentEmailAndStudentEmailAsync(string parentEmail, string studentEmail);
    }
}
