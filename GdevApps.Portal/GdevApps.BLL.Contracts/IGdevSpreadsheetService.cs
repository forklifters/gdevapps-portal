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
        Task<TaskResult<BoolResult, ICredential>> IsGradeBookAsync(
            string gradebookId, 
            string externalAccessToken, 
            string refreshToken, 
            string userId, 
            string gradebookLink = ""); 
        Task<TaskResult<BoolResult, ICredential>> IsGradeBookAsync(
            string gradebookId, 
            ICredential googleCredential, 
            string refreshToken, 
            string userId, 
            string gradebookLink = ""); 
        Task<TaskResult<IEnumerable<GradebookStudent>, ICredential>> GetStudentsFromGradebookAsync(
            string externalAccessToken, 
            string gradebookId, 
            string refreshToken, 
            string userId);
        Task<TaskResult<IEnumerable<GradebookStudent>, ICredential>> GetStudentsFromGradebookAsync(
            ICredential googleCredential, 
            string gradebookId, 
            string refreshToken, 
            string userId);
        Task<TaskResult<GradebookStudent, ICredential>> GetStudentByEmailFromGradebookAsync(
            string studentEmail, 
            string externalAccessToken, 
            string gradebookId, 
            string refreshToken, 
            string userId);
        Task<TaskResult<GradebookStudent, ICredential>> GetStudentByEmailFromGradebookAsync(
            string studentEmail, 
            ICredential googleCredential, 
            string gradebookId, 
            string refreshToken, 
            string userId);
        Task<TaskResult<string, ICredential>> SaveStudentIntoParentGradebookAsync(
            GradebookStudent student, 
            string externalAccessToken, 
            string refreshToken, 
            string userId, 
            string parentEmail, 
            string parentGradebookName);
        Task<TaskResult<string, ICredential>> SaveStudentIntoParentGradebookAsync(
            GradebookStudent student, 
            ICredential googleCredential, 
            string refreshToken, 
            string userId, 
            string parentEmail, 
            string parentGradebookName);
        Task<bool> AddGradebookAsync(GradeBook model);
        Task<bool> EditGradebookAsync(GradeBook model);
        Task<GradeBook> GetGradebookByUniqueIdAsync(string gradebookId);
        GradeBook GetGradebookByIdAsync(int id);
        Task<bool> DeleteGradeBookAsync(string classroomId, string gradebookId); 
        Task<List<GradeBook>> GetGradeBooksByClassIdAsync(string classId, string userEmail);
        Task<List<GradeBook>> GetAllGradeBooksAsync(string userId);
        Task<TaskResult<BoolResult, ICredential>> ShareGradeBookAsync(
            string externalAccessToken,
            string refreshToken,
            string userId,
            string parentEmail,
            string studentEmail,
            string className,
            string gradeBookId,
            string mainGradeBookId);
        Task<TaskResult<BoolResult, ICredential>> ShareGradeBookAsync(
            ICredential googleCredential,
            string refreshToken,
            string userId,
            string parentEmail,
            string studentEmail,
            string className,
            string gradeBookId,
            string mainGradeBookId);

        Task<TaskResult<BoolResult, ICredential>> UnShareGradeBookAsync(
            string externalAccessToken,
            string refreshToken,
            string userId,
            string parentEmail,
            string gradeBookId,
            string mainGradeBookId
        );
        Task<TaskResult<BoolResult, ICredential>> UnShareGradeBookAsync(
            ICredential googleCredential,
            string refreshToken,
            string userId,
            string parentEmail,
            string gradeBookId,
            string mainGradeBookId
        );

        Task<TaskResult<GradebookStudent, ICredential>> GetStudentInformationFromParentGradeBookAsync(
            string externalAccessToken,
            string refreshToken,
            string userId,
            string gradeBookId
        );

        Task<TaskResult<GradebookStudent, ICredential>> GetStudentInformationFromParentGradeBookAsync(
            ICredential googleCredential,
            string refreshToken,
            string userId,
            string gradeBookId
        );

        Task<TaskResult<GradebookSettings, ICredential>> GetSettingsFromParentGradeBookAsync(
                    string externalAccessToken,
                    string refreshToken,
                    string userId,
                    string gradeBookId
                );
        Task<TaskResult<GradebookSettings, ICredential>> GetSettingsFromParentGradeBookAsync(
                ICredential googleCredential,
                string refreshToken,
                string userId,
                string gradeBookId
            );

//TEST METHOD
        Task<TaskResult<BoolResult, ICredential>> CreateSpreadsheetAsync(
            ICredential googleCredential,
            string refreshToken,
            string userId
        );

        Task<TaskResult<BoolResult, ICredential>> CreateSpreadsheetAsync(
            string externalAccessToken,
            string refreshToken,
            string userId
        );

        GradebookStudentReport<StudentReport> GetStudentReportInformation(
            string externalAccessToken,
            string refreshToken,
            string userId,
            GradebookStudent student, 
            GradebookSettings settings);

        Task<List<GdevApps.BLL.Models.GDevClassroomService.GradeBook>> GetMainGradebooksByParentEmailAndStudentEmailAsync(string parentEmail, string studentEmail);

        Task<List<GradebookStudent>> GetGradebookStudentsByParentEmailAsync(string parentEmail);

        Task<string> GetParentGradebookUniqueIdByMainGradebookIdAsync(string mainGradebookId);
    }
}