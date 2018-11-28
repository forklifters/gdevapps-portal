using GdevApps.BLL.Models.AspNetUsers;
using GdevApps.BLL.Models.LicensedUser;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GdevApps.BLL.Contracts
{
    public interface IAspNetUserService
    {
        Task<IEnumerable<PortalUser>> GetAllUsersAsync();

        Task<IEnumerable<AspNetUserToken>> GetAllTokens();

        Task UpdateUserTokensAsync(AspNetUserToken userTokens);

        Task<IEnumerable<AspNetUserToken>> GetAllTokensByUserIdAsync(string userId);
        IEnumerable<AspNetUserToken> GetAllTokensByUserId(string userId);
        bool AddParent(Parent parent);

        Task<Parent> GetParentByEmailAsync(string email);

        Task<Teacher> GetTeacherByEmailAsync(string email);

        Task<Users> GetLicensedUserByEmailAsync(string email);

        bool AddUserLogin(AspNetUserLogin userLogin);

        Task<bool> SetParentAspUserId(int parentId, string aspUserId);

        Task<bool> SetTeacherAspUserId(int teacherId, string aspUserId);
        Task<List<ParentModel>> GetAllParentsByTeacherAsync(string aspUserTeacherId);

        bool DeleteParentById(int id);
        bool DeleteTeacherById(int id);
    }
}
