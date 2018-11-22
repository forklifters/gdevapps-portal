using System.Collections.Generic;
using System.Threading.Tasks;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using GdevApps.DAL.Repositories.BaseRepository;

namespace GdevApps.DAL.Repositories.AspNetUserRepository
{
    public interface IAspNetUserRepository : IRepository, IReadOnlyRepository
    {
        Task<List<Parent>> GetParentsByCreatorIdTempAsync(string creatorId);
    }
}
