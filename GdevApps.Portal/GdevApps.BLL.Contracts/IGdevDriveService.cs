using System.Collections.Generic;
using System.Threading.Tasks;
using GdevApps.BLL.Models;
using GdevApps.BLL.Models.GDevClassroomService;
using Google.Apis.Auth.OAuth2;

namespace GdevApps.BLL.Contracts
{
    public interface IGdevDriveService
    {
        //returns folder Id
        Task<TaskResult<string, ICredential>> CreateRootFolderAsync(string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<string, ICredential>> CreateRootFolderAsync(ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<string, ICredential>> CreateInnerFolderAsync(string rootFolderId, string folderName, string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<string, ICredential>> CreateInnerFolderAsync(string rootFolderId, string folderName, ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<BoolResult, ICredential>> IsFileExistsAsync(string fileId, string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<BoolResult, ICredential>> IsFileExistsAsync(string fileId, ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<string, ICredential>> GetRootFolderIdAsync(string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<string, ICredential>> GetRootFolderIdAsync(ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<BoolResult, ICredential>> MoveFileToFolderAsync(string fileId, string folderId, string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<BoolResult, ICredential>> MoveFileToFolderAsync(string fileId, string folderId, ICredential googleCredential, string refreshToken, string userId);
    }
}