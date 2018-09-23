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
        Task<TaskResult<IEnumerable<GoogleStudent>, ICredential>> GetStudentsByClassIdAndGradebookIdAsync(string externalAccessToken, string classId, string gradebookId, string refreshToken, string userId);
        Task<TaskResult<IEnumerable<GoogleStudent>, ICredential>> GetStudentsByClassIdAndGradebookIdAsync(ICredential googleCredential, string classId, string gradebookId, string refreshToken, string userId);
        Task<TaskResult<GoogleStudent, ICredential>> GetStudentByIdAsync(string studentId, string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<GoogleStudent, ICredential>> GetStudentByIdAsync(string studentId, ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<BoolResult, ICredential>> AddGradebookAsync(string classroomId);
        Task<TaskResult<BoolResult, ICredential>> EditGradebookAsync(Gradebook model);
        Task<TaskResult<Gradebook, ICredential>> GetGradebookByIdAsync(string classroomId, string gradebookId);
        Task<TaskResult<BoolResult, ICredential>> DeleteGradeBookAsync(string classroomId, string gradebookId); 
    }
}