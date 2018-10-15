using System.Collections.Generic;
using System.Threading.Tasks;
using GdevApps.BLL.Models;
using GdevApps.BLL.Models.GDevClassroomService;
using GdevApps.BLL.Models.GDevSpreadSheetService;
using Google.Apis.Auth.OAuth2;

namespace GdevApps.BLL.Contracts
{
    public interface IGdevSpreadsheetService
    {
        Task<TaskResult<BoolResult, ICredential>> IsGradeBookAsync(string gradebookId, string externalAccessToken, string refreshToken, string userId, string gradebookLink = ""); 
        Task<TaskResult<BoolResult, ICredential>> IsGradeBookAsync(string gradebookId, ICredential googleCredential, string refreshToken, string userId, string gradebookLink = ""); 
        Task<TaskResult<IEnumerable<GradebookStudent>, ICredential>> GetStudentsFromGradebookAsync(string externalAccessToken, string gradebookId, string refreshToken, string userId);
        Task<TaskResult<IEnumerable<GradebookStudent>, ICredential>> GetStudentsFromGradebookAsync(ICredential googleCredential, string gradebookId, string refreshToken, string userId);
        Task<TaskResult<GradebookStudent, ICredential>> GetStudentByEmailFromGradebookAsync(string studentEmail, string externalAccessToken, string gradebookId, string refreshToken, string userId);
        Task<TaskResult<GradebookStudent, ICredential>> GetStudentByEmailFromGradebookAsync(string studentEmail, ICredential googleCredential, string gradebookId, string refreshToken, string userId);
        Task<TaskResult<string, ICredential>> SaveStudentIntoParentGradebookAsync(GradebookStudent student, string externalAccessToken, string refreshToken, string userId);
        
        //TODO: Add one more parameter parentEmail and add item into the ParentGradebook for a single parent at the end. Return parentGradebookId
        Task<TaskResult<string, ICredential>> SaveStudentIntoParentGradebookAsync(GradebookStudent student, ICredential googleCredential, string refreshToken, string userId);
        bool AddGradebook(GradeBook model);
        Task<TaskResult<BoolResult, ICredential>> EditGradebookAsync(GradeBook model);
        Task<GradeBook> GetGradebookByUniqueIdAsync(string gradebookId);
        GradeBook GetGradebookByIdAsync(int id);
        Task<bool> DeleteGradeBookAsync(string classroomId, string gradebookId); 
        Task<IEnumerable<GradeBook>> GetGradeBooksByClassId(string classId);

        
    }
}