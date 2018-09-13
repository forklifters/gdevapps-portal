using System.Collections.Generic;
using System.Threading.Tasks;
using GdevApps.BLL.Models.GDevClassroomService;

namespace GdevApps.BLL.Contracts
{
    public interface IGdevClassroomService
    {
        Task<List<GoogleClass>> GetAllClassesAsync(string externalAccessToken, string refreshToken, string userId);
        Task<List<GoogleStudent>> GetStudentsByClassIdAndGradebookIdAsync(string externalAccessToken, string classId, string gradebookId, string refreshToken, string userId);
        Task<GoogleStudent> GetStudentById(string studentId);
        Task AddGradebookAsync(string classroomId);
        Task EditGradebookAsync(Gradebook model);
        Task<Gradebook> GetGradebookByIdAsync(string classroomId, string gradebookId);
        Task DeleteGradeBookAsync(string classroomId, string gradebookId); 
    }
}