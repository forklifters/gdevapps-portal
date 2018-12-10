using GdevApps.BLL.Models.AspNetUsers;
using GdevApps.BLL.Models.LicensedUser;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GdevApps.BLL.Contracts
{
    public interface IAspNetUserService
    {
        Task<List<PortalUser>> GetAllUsersAsync();

        Task UpdateUserTokensAsync(AspNetUserToken userTokens);

        Task<IEnumerable<AspNetUserToken>> GetAllTokensByUserIdAsync(string userId);
        IEnumerable<AspNetUserToken> GetAllTokensByUserId(string userId);
        Task<bool> AddParentAsync(Parent parent);
        Task<bool> AddTeacherAsync(Teacher parent);

        Task<Parent> GetParentByEmailAsync(string email);

        Task<Teacher> GetTeacherByEmailAsync(string email);

        Task<Users> GetLicensedUserByEmailAsync(string email);

        bool AddUserLogin(AspNetUserLogin userLogin);

        Task<bool> SetParentAspUserIdAsync(int parentId, string aspUserId);

        Task<bool> SetTeacherAspUserIdAsync(int teacherId, string aspUserId);
        Task<List<ParentModel>> GetAllParentsByTeacherAsync(string aspUserTeacherId);
        Task<bool> DeleteParentByIdAsync(int id);
        Task<bool> DeleteTeacherByIdAsync(int id);
        Task<Parent> GetParentByIdAsync(int id);
        Task<Teacher> GetTeacherByIdAsync(int id);
        Task<List<Parent>> GetAllParentsAsync();
        Task<List<Teacher>> GetAllTeachersAsync();
    }
}
