using System.Collections.Generic;
using System.Threading.Tasks;
using GdevApps.BLL.Models;
using GdevApps.BLL.Models.GDevClassroomService;
using GdevApps.BLL.Models.GDevDriveService;
using Google.Apis.Auth.OAuth2;

namespace GdevApps.BLL.Contracts
{
    public interface IGdevDriveService
    {
        Task<TaskResult<string, ICredential>> CreateRootFolderAsync(string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<string, ICredential>> CreateRootFolderAsync(ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<Folder, ICredential>> CreateInnerFolderAsync(string rootFolderId, string folderName, string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<Folder, ICredential>> CreateInnerFolderAsync(string rootFolderId, string folderName, ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<FileStateResult, ICredential>> IsFileExistsAsync(string fileId, string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<FileStateResult, ICredential>> IsFileExistsAsync(string fileId, ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<string, ICredential>> GetRootFolderIdAsync(string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<string, ICredential>> GetRootFolderIdAsync(ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<BoolResult, ICredential>> MoveFileToFolderAsync(string fileId, string folderId, string externalAccessToken, string refreshToken, string userId);
        Task<TaskResult<BoolResult, ICredential>> MoveFileToFolderAsync(string fileId, string folderId, ICredential googleCredential, string refreshToken, string userId);
        Task<TaskResult<string, ICredential>> GetInnerFolderIdAsync(string externalAccessToken, string refreshToken, string userId, string rootFolderId, string folderName);
        Task<TaskResult<string, ICredential>> GetInnerFolderIdAsync(ICredential googleCredential, string refreshToken, string userId, string rootFolderId, string folderName);
        Task<TaskResult<Folder, ICredential>> GetInnerFolderAsync(string externalAccessToken, string refreshToken, string userId, string rootFolderId, string folderName);
        Task<TaskResult<Folder, ICredential>> GetInnerFolderAsync(ICredential googleCredential, string refreshToken, string userId, string rootFolderId, string folderName);
        Task<TaskResult<BoolResult, ICredential>> GrantPermission(string externalAccessToken, string refreshToken, string userId, string fileId, string email, string permissionType, string role);
        Task<TaskResult<BoolResult, ICredential>> GrantPermission(ICredential googleCredential, string refreshToken, string userId, string fileId, string email, string permissionType, string role);
        
        //Permissions can not be deleted by email in v3 https://stackoverflow.com/questions/14148021/any-way-to-get-email-address-from-google-drive-api-permissions-so-i-can-differen
        //https://developers.google.com/drive/api/v3/migration
        Task<TaskResult<BoolResult, ICredential>> DeletePermissionAsync(string externalAccessToken, string refreshToken, string userId, string fileId, string permissionType, string role);
        Task<TaskResult<BoolResult, ICredential>> DeletePermissionAsync(ICredential googleCredential, string refreshToken, string userId, string fileId, string permissionType, string role);
        Task<bool> DeleteRootFolderAsync(string googleFolderId);
        Task<bool> DeleteInnerFolderAsync(string googleFolderId);
    }
}