using System.Collections.Generic;
using System.Threading.Tasks;
using GdevApps.BLL.Models.GDevClassroomService;

namespace GdevApps.BLL.Contracts
{
    public interface IGdevSpreadsheetService
    {
        Task<bool> IsGradeBook(string gradebookId, string externalAccessToken, string refreshToken, string userId, string gradebookLink = ""); 
        Task<IEnumerable<GradebookStudent>> GetStudentsFromGradebook(string externalAccessToken, string gradebookId, string refreshToken, string userId);
    }
}