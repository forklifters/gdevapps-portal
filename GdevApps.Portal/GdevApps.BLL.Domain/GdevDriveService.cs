using System;
using System.Linq;
using System.Threading.Tasks;
using GdevApps.BLL.Contracts;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using File = Google.Apis.Drive.v3.Data.File;
using GdevApps.BLL.Models;
using GdevApps.DAL.Repositories.GradeBookRepository;
using GdevApps.DAL.DataModels.AspNetUsers.GradeBook;

namespace GdevApps.BLL.Domain
{
    public class GdevDriveService : IGdevDriveService
    {
        private const string _mainFolderName = "Gdevapps Google Portal";
        private readonly IAspNetUserService _aspUserService;
        private readonly IConfiguration _configuration;
        private readonly IGradeBookRepository _gradeBookRepository;

        public GdevDriveService(IAspNetUserService aspUserService, IConfiguration configuration, IGradeBookRepository gradeBookRepository)
        {
            _aspUserService = aspUserService;
            _configuration = configuration;
            _gradeBookRepository = gradeBookRepository;
        }

        public async Task<TaskResult<string, ICredential>> CreateRootFolderAsync(string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await CreateRootFolderAsync(googleCredential, refreshToken, userId);
        }
        public async Task<TaskResult<string, ICredential>> CreateInnerFolderAsync(string rootFolderId, string folderName, string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await CreateInnerFolderAsync(rootFolderId, folderName, googleCredential, refreshToken, userId);
        }
        public async Task<TaskResult<BoolResult, ICredential>> IsFileExistsAsync(string fileId, string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await IsFileExistsAsync(fileId, googleCredential, refreshToken, userId);
        }
        public async Task<TaskResult<string, ICredential>> GetRootFolderIdAsync(string externalAccessToken, string refreshToken, string userId)
        {
            //find folder id using repository
            var rootFolder = await _gradeBookRepository.GetOneAsync<Folder>(filter: f=> f.CreatedBy == userId && f.FolderType == (int)FolderType.ROOT);
            if (rootFolder == null || string.IsNullOrEmpty(rootFolder.GoogleFileId))
            {
                return await CreateRootFolderAsync(externalAccessToken, refreshToken, userId);
            }
            var rootFolderId = rootFolder.GoogleFileId;

            return new TaskResult<string, ICredential>(ResultType.SUCCESS, rootFolderId, GoogleCredential.FromAccessToken(externalAccessToken));
        }
        public async Task<TaskResult<BoolResult, ICredential>> MoveFileToFolderAsync(string fileId, string folderId, string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await MoveFileToFolderAsync(fileId, folderId, googleCredential, refreshToken, userId);
        }
        public async Task<TaskResult<string, ICredential>> CreateRootFolderAsync(ICredential googleCredential, string refreshToken, string userId)
        {
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"],
            });

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = _mainFolderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            FilesResource.CreateRequest request;
            Google.Apis.Drive.v3.Data.File file;
            try
            {
                request = driveService.Files.Create(fileMetadata);
                file = await request.ExecuteAsync();
                //save folder Id and name into the database
                var rootFolder = new Folder(){
                    FolderName = _mainFolderName,
                    GoogleFileId = file.Id,
                    FolderType = (int)FolderType.ROOT,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                _gradeBookRepository.Create<Folder>(rootFolder);
                _gradeBookRepository.Save();

                return new TaskResult<string, ICredential>(ResultType.SUCCESS, file.Id, googleCredential);
            }
            catch (Google.GoogleApiException exception)
            {
                switch (exception?.Error?.Code)
                {
                    case 401:
                        var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse { RefreshToken = refreshToken };
                        googleCredential = new UserCredential(new GoogleAuthorizationCodeFlow(
                           new GoogleAuthorizationCodeFlow.Initializer
                           {
                               ClientSecrets = new ClientSecrets
                               {
                                   ClientId = _configuration["installed:client_id"],
                                   ClientSecret = _configuration["installed:client_secret"]
                               }
                           }), "user", token);

                        driveService = new DriveService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = _configuration["ApplicationName"]
                        });

                        request = driveService.Files.Create(fileMetadata);
                        file = await request.ExecuteAsync();
                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        return new TaskResult<string, ICredential>(ResultType.SUCCESS, file.Id, googleCredential);

                    default: throw exception;
                }
            }
        }
        public async Task<TaskResult<string, ICredential>> CreateInnerFolderAsync(string rootFolderId, string folderName, ICredential googleCredential, string refreshToken, string userId)
        {
            
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"],
            });

            FilesResource.GetRequest getRootFolderRequest;
            Google.Apis.Drive.v3.Data.File rootFolder;
            try
            {
                getRootFolderRequest = driveService.Files.Get(rootFolderId);
                rootFolder = await getRootFolderRequest.ExecuteAsync();
            }
            catch (Google.GoogleApiException exception)
            {
                switch (exception?.Error?.Code)
                {
                    case 401:
                        var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse { RefreshToken = refreshToken };
                        googleCredential = new UserCredential(new GoogleAuthorizationCodeFlow(
                           new GoogleAuthorizationCodeFlow.Initializer
                           {
                               ClientSecrets = new ClientSecrets
                               {
                                   ClientId = _configuration["installed:client_id"],
                                   ClientSecret = _configuration["installed:client_secret"]
                               }
                           }), "user", token);

                        driveService = new DriveService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = _configuration["ApplicationName"]
                        });

                        getRootFolderRequest = driveService.Files.Get(rootFolderId);
                        rootFolder = await getRootFolderRequest.ExecuteAsync();
                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        break;
                    default: throw exception;
                }
            }
            //TODO: get the folders ids from the db by folderName.
            //if there any - check if they are still exist
            // if no -create one

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string>() { rootFolder.Id }
            };

            Google.Apis.Drive.v3.Data.File file = null;
            if (file == null)
            {
                FilesResource.CreateRequest request = driveService.Files.Create(fileMetadata);
                file = await request.ExecuteAsync();
                var innerFolder = new Folder()
                {
                    FolderName = _mainFolderName,
                    GoogleFileId = file.Id,
                    FolderType = (int)FolderType.INNER,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                _gradeBookRepository.Create<Folder>(innerFolder);
                _gradeBookRepository.Save();
            }

            return new TaskResult<string, ICredential>(ResultType.SUCCESS, file.Id, googleCredential);
        }
        public async Task<TaskResult<BoolResult, ICredential>> IsFileExistsAsync(string fileId, ICredential googleCredential, string refreshToken, string userId)
        {
            FilesResource.GetRequest request;
            Google.Apis.Drive.v3.Data.File file;
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"],
            });

            try
            {
                request = driveService.Files.Get(fileId);
                file = await request.ExecuteAsync();
                return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(file.Id != null), googleCredential);
            }
            catch (Google.GoogleApiException exception)
            {
                switch (exception?.Error?.Code)
                {
                    case 404:
                        return new TaskResult<BoolResult, ICredential>(ResultType.ERROR, new BoolResult(false), googleCredential);
                    case 401:
                        var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse { RefreshToken = refreshToken };
                        googleCredential = new UserCredential(new GoogleAuthorizationCodeFlow(
                           new GoogleAuthorizationCodeFlow.Initializer
                           {
                               ClientSecrets = new ClientSecrets
                               {
                                   ClientId = _configuration["installed:client_id"],
                                   ClientSecret = _configuration["installed:client_secret"]
                               }
                           }), "user", token);

                        driveService = new DriveService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = _configuration["ApplicationName"]
                        });

                        request = driveService.Files.Get(fileId);
                        file = await request.ExecuteAsync();
                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(file.Id != null), googleCredential);

                    default: 
                    return new TaskResult<BoolResult, ICredential>(ResultType.ERROR, new BoolResult(false), googleCredential);
                }
            }
            catch (Exception err)
            {
                return new TaskResult<BoolResult, ICredential>(ResultType.ERROR, new BoolResult(false), googleCredential);
            }
        }
        public async Task<TaskResult<string, ICredential>> GetRootFolderIdAsync(ICredential googleCredential, string refreshToken, string userId)
        {
            var rootFolderId = "";//_driveRepository.GetFolderRoot<Folder>(filter: x=>x.Type == "Root") 
            if (string.IsNullOrEmpty(rootFolderId))
            {
                return await CreateRootFolderAsync(googleCredential, refreshToken, userId);
            }

            return new TaskResult<string, ICredential>(ResultType.SUCCESS, rootFolderId, googleCredential);
        }
        public async Task<TaskResult<BoolResult, ICredential>> MoveFileToFolderAsync(string fileId, string folderId, ICredential googleCredential, string refreshToken, string userId)
        {
            FilesResource.GetRequest request;
            File file;
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"],
            });
            try
            {
                request = driveService.Files.Get(fileId);
                request.Fields = "parents";
                file = await request.ExecuteAsync();
            }
            catch (Google.GoogleApiException exception)
            {
                switch (exception?.Error?.Code)
                {
                    case 401:
                        var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse { RefreshToken = refreshToken };
                        googleCredential = new UserCredential(new GoogleAuthorizationCodeFlow(
                           new GoogleAuthorizationCodeFlow.Initializer
                           {
                               ClientSecrets = new ClientSecrets
                               {
                                   ClientId = _configuration["installed:client_id"],
                                   ClientSecret = _configuration["installed:client_secret"]
                               }
                           }), "user", token);

                        driveService = new DriveService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = _configuration["ApplicationName"]
                        });

                        request = driveService.Files.Get(fileId);
                        file = await request.ExecuteAsync();
                        break;
                    default: throw exception;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            var previousParents = string.Join(",", file.Parents);
            var updateRequest = driveService.Files.Update(new File(), fileId);
            updateRequest.Fields = "id, parents";
            updateRequest.AddParents = folderId;
            updateRequest.RemoveParents = previousParents;
            file = await updateRequest.ExecuteAsync();

            return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(!string.IsNullOrEmpty(file.Id)), googleCredential);
        }

        #region Private methods
        private async Task<Google.Apis.Drive.v3.Data.File> InsertFile(DriveService service, string title, string description, string parentId, string mimeType, string filename)
        {
            // File's metadata.
            Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
            body.Name = title;
            body.Description = description;
            body.MimeType = mimeType;

            // Set the parent folder.
            if (!String.IsNullOrEmpty(parentId))
            {
                body.Parents = new List<string>() { parentId };
            }

            // File's content.
            byte[] byteArray = System.IO.File.ReadAllBytes(filename);
            MemoryStream stream = new MemoryStream(byteArray);

            try
            {
                FilesResource.CreateMediaUpload request = service.Files.Create(body, stream, mimeType);
                await request.UploadAsync();
                Google.Apis.Drive.v3.Data.File file = request.ResponseBody;

                return file;
            }
            catch (Exception e)
            {
                //Console.WriteLine("An error occurred: " + e.Message);
                //Log the error
                return null;
            }
        }
        private async Task<List<Google.Apis.Drive.v3.Data.File>> RetrieveAllFilesAsync(DriveService service)
        {
            List<Google.Apis.Drive.v3.Data.File> result = new List<Google.Apis.Drive.v3.Data.File>();
            FilesResource.ListRequest request = service.Files.List();
            do
            {
                try
                {
                    Google.Apis.Drive.v3.Data.FileList files = await request.ExecuteAsync();
                    result.AddRange(files.Files);
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    //Console.WriteLine("An error occurred: " + e.Message);
                    //Log error
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));

            return result;
        }
        private async Task UpdateAllTokens(string userId, UserCredential credentials)
        {
            var userLoginTokens = await _aspUserService.GetAllTokensByUserIdAsync(userId);
            if (userLoginTokens != null)
            {
                var accessTokenRecord = userLoginTokens.Where(t => t.Name == "access_token").First();
                accessTokenRecord.Value = credentials.Token.AccessToken;
                await _aspUserService.UpdateUserTokensAsync(accessTokenRecord);

                var expiresAtTokenRecord = userLoginTokens.Where(t => t.Name == "expires_at").FirstOrDefault();

                var issuedDate = credentials.Token.IssuedUtc;
                if (credentials.Token.ExpiresInSeconds.HasValue)
                {
                    expiresAtTokenRecord.Value = issuedDate.AddSeconds(credentials.Token.ExpiresInSeconds.Value).ToString("o",
                     System.Globalization.CultureInfo.InvariantCulture);
                    await _aspUserService.UpdateUserTokensAsync(expiresAtTokenRecord);
                }

                var tokenUpdatedRecord = userLoginTokens.Where(t => t.Name == "token_updated").First();
                tokenUpdatedRecord.Value = "true";
                await _aspUserService.UpdateUserTokensAsync(tokenUpdatedRecord);

                var tokenUpdatedTimeRecord = userLoginTokens.Where(t => t.Name == "token_updated_time").First();
                tokenUpdatedTimeRecord.Value = DateTime.UtcNow.ToString();
                await _aspUserService.UpdateUserTokensAsync(tokenUpdatedRecord);
            }
        }
        #endregion
    }
}