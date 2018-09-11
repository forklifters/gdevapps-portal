using System.Collections.Generic;
using System.Threading.Tasks;
using GdevApps.BLL.Models.GDevClassroomService;

namespace GdevApps.BLL.Contracts
{
    public interface IGdevClassroomService
    {
        Task<List<GoogleClass>> GetAllClassesAsync(string externalAccessToken, string refreshToken);
        Task<List<GoogleStudent>> GetStudentsByClassIdAndGradebookIdAsync(string externalAccessToken, string classId, string gradebookId);
        Task<GoogleStudent> GetStudentById(string studentId);
        Task AddGradebookAsync(string classroomId);
        Task EditGradebookAsync(Gradebook model);
        Task<Gradebook> GetGradebookByIdAsync(string classroomId, string gradebookId);
        Task DeleteGradeBookAsync(string classroomId, string gradebookId); 
    }
}