using System.Collections.Generic;
using System.Threading.Tasks;
using GdevApps.BLL.Models;
using GdevApps.BLL.Models.GDevClassroomService;
using Google.Apis.Auth.OAuth2;

namespace GdevApps.BLL.Contracts
{
    public interface IGdevClassroomService
    {
        Task<TaskResult<IEnumerable<GoogleClass>, ICredential>> GetAllClassesAsync(string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<IEnumerable<GoogleClass>, ICredential>> GetAllClassesAsync(ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<GoogleClass, ICredential>> GetClassByIdAsync(string classroomId, string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<GoogleClass, ICredential>> GetClassByIdAsync(string classroomId, ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<IEnumerable<GoogleStudent>, ICredential>> GetStudentsByClassIdAsync(string externalAccessToken, string classId, string refreshToken, string userId);
        Task<TaskResult<IEnumerable<GoogleStudent>, ICredential>> GetStudentsByClassIdAsync(ICredential googleCredential, string classId, string refreshToken, string userId);
        Task<TaskResult<GoogleStudent, ICredential>> GetStudentByIdAsync(string studentId, string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<GoogleStudent, ICredential>> GetStudentByIdAsync(string studentId, ICredential googleCredential, string refreshToken, string userId);
    }
}