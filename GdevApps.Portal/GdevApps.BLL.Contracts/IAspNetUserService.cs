using GdevApps.BLL.Models.AspNetUsers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GdevApps.BLL.Contracts
{
    public interface IAspNetUserService
    {
        Task<IEnumerable<Models.AspNetUsers.AspNetUser>> GetAllUsersAsync();

        Task<IEnumerable<AspNetUserToken>> GetAllTokens();
    }
}
