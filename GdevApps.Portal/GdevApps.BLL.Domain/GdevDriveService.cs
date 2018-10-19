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
using AutoMapper;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Requests;
using System.Collections.Concurrent;
using GdevApps.BLL.Models.GDevDriveService;

namespace GdevApps.BLL.Domain
{
    public class GdevDriveService : IGdevDriveService
    {
        private const string _mainFolderName = "Gdevapps Google Portal";
        private readonly IAspNetUserService _aspUserService;
        private readonly IConfiguration _configuration;
        private readonly IGradeBookRepository _gradeBookRepository;
        private readonly IMapper _mapper;


        public GdevDriveService(IAspNetUserService aspUserService, IConfiguration configuration, IGradeBookRepository gradeBookRepository, IMapper mapper)
        {
            _aspUserService = aspUserService;
            _configuration = configuration;
            _gradeBookRepository = gradeBookRepository;
            _mapper = mapper;
        }

        public async Task<TaskResult<string, ICredential>> CreateRootFolderAsync(string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await CreateRootFolderAsync(googleCredential, refreshToken, userId);
        }
        public async Task<TaskResult<GdevApps.BLL.Models.GDevDriveService.Folder, ICredential>> CreateInnerFolderAsync(string rootFolderId, string folderName, string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await CreateInnerFolderAsync(rootFolderId, folderName, googleCredential, refreshToken, userId);
        }
        public async Task<TaskResult<FileStateResult, ICredential>> IsFileExistsAsync(string fileId, string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await IsFileExistsAsync(fileId, googleCredential, refreshToken, userId);
        }
        public async Task<TaskResult<string, ICredential>> GetRootFolderIdAsync(string externalAccessToken, string refreshToken, string userId)
        {
            //find folder id using repository
            var rootFolder = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(
                filter: f => f.CreatedBy == userId && 
                f.FolderType == (int)FolderType.ROOT && 
                f.PrentFolderId == null &&
                !f.IsDeleted);
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
                var rootFolder = new GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder()
                {
                    FolderName = _mainFolderName,
                    GoogleFileId = file.Id,
                    FolderType = (int)FolderType.ROOT,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    PrentFolderId = null
                };
                _gradeBookRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(rootFolder);
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
        public async Task<TaskResult<GdevApps.BLL.Models.GDevDriveService.Folder, ICredential>> CreateInnerFolderAsync(string rootFolderId, string folderName, ICredential googleCredential, string refreshToken, string userId)
        {
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"],
            });

            // check if folder already exists. If yes return Error
            var innerFolderResult = await GetInnerFolderIdAsync(googleCredential, refreshToken, userId, rootFolderId, folderName);
            if (innerFolderResult.Result == ResultType.SUCCESS && !string.IsNullOrWhiteSpace(innerFolderResult.ResultObject))
            {
                throw new Exception("Folder already exists");
            }

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

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string>() { rootFolder.Id }
            };

            var rootFolderDal = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(filter: f => f.GoogleFileId == rootFolderId);
            FilesResource.CreateRequest request = driveService.Files.Create(fileMetadata);
            Google.Apis.Drive.v3.Data.File file = await request.ExecuteAsync();
            var innerFolder = new GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder()
            {
                FolderName = folderName,
                GoogleFileId = file.Id,
                FolderType = (int)FolderType.INNER,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false,
                PrentFolderId = rootFolderDal.Id
            };
            _gradeBookRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(innerFolder);
            _gradeBookRepository.Save();

            var innderFodlerBll = _mapper.Map<GdevApps.BLL.Models.GDevDriveService.Folder>(innerFolder);
            return new TaskResult<GdevApps.BLL.Models.GDevDriveService.Folder, ICredential>(ResultType.SUCCESS, innderFodlerBll, googleCredential);
        }

        //TODO: Return new result TRASHED
        public async Task<TaskResult<FileStateResult, ICredential>> IsFileExistsAsync(string fileId, ICredential googleCredential, string refreshToken, string userId)
        {
            if(string.IsNullOrWhiteSpace(fileId))
                return new TaskResult<FileStateResult, ICredential>(ResultType.SUCCESS, new FileStateResult(FileState.NOTEXIST), googleCredential);

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
                request.Fields = "trashed, id";
                file = await request.ExecuteAsync();
                if (file?.Trashed ?? false)
                {
                    return new TaskResult<FileStateResult, ICredential>(ResultType.SUCCESS, new FileStateResult(FileState.TRASHED), googleCredential);
                }
                else if (file.Id != null)
                {
                    return new TaskResult<FileStateResult, ICredential>(ResultType.SUCCESS, new FileStateResult(FileState.EXISTS), googleCredential);
                }
                else
                {
                    return new TaskResult<FileStateResult, ICredential>(ResultType.SUCCESS, new FileStateResult(FileState.NOTEXIST), googleCredential);
                }
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
                        request.Fields = "trashed, id";
                        file = await request.ExecuteAsync();
                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        bool exists = (file?.Trashed ?? false) && file.Id != null;
                        if (file?.Trashed ?? false)
                        {
                            return new TaskResult<FileStateResult, ICredential>(ResultType.SUCCESS, new FileStateResult(FileState.TRASHED), googleCredential);
                        }
                        else if (file.Id != null)
                        {
                            return new TaskResult<FileStateResult, ICredential>(ResultType.SUCCESS, new FileStateResult(FileState.EXISTS), googleCredential);
                        }
                        else
                        {
                            return new TaskResult<FileStateResult, ICredential>(ResultType.SUCCESS, new FileStateResult(FileState.NOTEXIST), googleCredential);
                        }

                    default:
                        throw exception;
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }
        public async Task<TaskResult<string, ICredential>> GetRootFolderIdAsync(ICredential googleCredential, string refreshToken, string userId)
        {
            var rootFolder = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(filter: x => x.FolderType == (int)FolderType.ROOT && x.PrentFolderId == null);

            if (string.IsNullOrEmpty(rootFolder.GoogleFileId))
            {
                return await CreateRootFolderAsync(googleCredential, refreshToken, userId);
            }

            return new TaskResult<string, ICredential>(ResultType.SUCCESS, rootFolder.GoogleFileId, googleCredential);
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

            try
            {
                var previousParents = string.Join(",", file.Parents);
                var updateRequest = driveService.Files.Update(new File(), fileId);
                updateRequest.Fields = "id, parents";
                updateRequest.AddParents = folderId;
                updateRequest.RemoveParents = previousParents;
                file = await updateRequest.ExecuteAsync();

                return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(!string.IsNullOrEmpty(file.Id)), googleCredential);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<TaskResult<string, ICredential>> GetInnerFolderIdAsync(string externalAccessToken, string refreshToken, string userId, string rootFolderId, string folderName)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            var innerFolerResult = await GetInnerFolderAsync(googleCredential, refreshToken, userId, rootFolderId, folderName);
            if (innerFolerResult.Result == ResultType.SUCCESS && innerFolerResult.ResultObject != null)
            {
                return new TaskResult<string, ICredential>(ResultType.SUCCESS, innerFolerResult.ResultObject.GoogleFileId, googleCredential); ;

            }
            else
            {
                return new TaskResult<string, ICredential>(ResultType.EMPTY, "", googleCredential); ;
            }
        }

        public async Task<TaskResult<string, ICredential>> GetInnerFolderIdAsync(ICredential googleCredential, string refreshToken, string userId, string rootFolderId, string folderName)
        {
            var innerFolerResult = await GetInnerFolderAsync(googleCredential, refreshToken, userId, rootFolderId, folderName);
            if (innerFolerResult.Result == ResultType.SUCCESS && innerFolerResult.ResultObject != null)
            {
                return new TaskResult<string, ICredential>(ResultType.SUCCESS, innerFolerResult.ResultObject.GoogleFileId, googleCredential); ;
            }
            else
            {
                return new TaskResult<string, ICredential>(ResultType.EMPTY, "", googleCredential); ;
            }
        }

        public async Task<TaskResult<GdevApps.BLL.Models.GDevDriveService.Folder, ICredential>> GetInnerFolderAsync(string externalAccessToken, string refreshToken, string userId, string rootFolderId, string folderName)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await GetInnerFolderAsync(googleCredential, refreshToken, userId, rootFolderId, folderName);
        }

        public async Task<TaskResult<GdevApps.BLL.Models.GDevDriveService.Folder, ICredential>> GetInnerFolderAsync(ICredential googleCredential, string refreshToken, string userId, string rootFolderId, string folderName)
        {
            var parentFolder = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(filter: f => f.GoogleFileId == rootFolderId && !f.IsDeleted);
            var innerFolder = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(
                filter: f => f.PrentFolderId == parentFolder.Id && 
                                f.FolderName == folderName && 
                                f.FolderType == (int)FolderType.INNER && 
                                f.PrentFolderId != null &&
                                !f.IsDeleted
                );
            var innerFolderBll = _mapper.Map<GdevApps.BLL.Models.GDevDriveService.Folder>(innerFolder);
            if (innerFolder != null)
            {
                return new TaskResult<GdevApps.BLL.Models.GDevDriveService.Folder, ICredential>(ResultType.SUCCESS, innerFolderBll, googleCredential); ;
            }
            else
            {
                return new TaskResult<GdevApps.BLL.Models.GDevDriveService.Folder, ICredential>(ResultType.EMPTY, null, googleCredential); ;
            }
        }


        public async Task<TaskResult<BoolResult, ICredential>> GrantPermission(string externalAccessToken, string refreshToken, string userId, string fileId, string email, string permissionType, string role)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await GrantPermission(googleCredential, refreshToken, userId, fileId, email, permissionType, role);
        }

        public async Task<TaskResult<BoolResult, ICredential>> GrantPermission(ICredential googleCredential, string refreshToken, string userId, string fileId, string email, string permissionType, string role)
        {
            PermissionsResource.CreateRequest request;
            Permission permissionResult;
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"],
            });

            var permission = new Permission()
            {
                Type = permissionType,
                Role = role,
                EmailAddress = email
            };

            try
            {
                request = driveService.Permissions.Create(permission, fileId);
                //request.Fields = "id";
                permissionResult = await request.ExecuteAsync();
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

                        request = driveService.Permissions.Create(permission, fileId);
                        //request.Fields = "id";
                        permissionResult = await request.ExecuteAsync();
                        break;
                    default: throw exception;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(true), googleCredential);
        }


        public async Task<TaskResult<BoolResult, ICredential>> DeletePermissionAsync(string externalAccessToken, string refreshToken, string userId, string fileId, string permissionType = "", string role = "")
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await DeletePermissionAsync(googleCredential, refreshToken, userId, fileId, permissionType, role);
        }

        public async Task<TaskResult<BoolResult, ICredential>> DeletePermissionAsync(ICredential googleCredential, string refreshToken, string userId, string fileId, string permissionType = "", string role = "")
        {
            PermissionsResource.ListRequest requestList;
            PermissionList permissionsResult;
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"],
            });

            try
            {
                requestList = driveService.Permissions.List(fileId);
                permissionsResult = await requestList.ExecuteAsync();
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

                        requestList = driveService.Permissions.List(fileId);
                        permissionsResult = await requestList.ExecuteAsync();
                        break;
                    default: throw exception;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            var permissionToDeleteQuery = permissionsResult.Permissions.Where(p => p.Role != PermissionRole.Owner);
            if (!string.IsNullOrWhiteSpace(role))
            {
                permissionToDeleteQuery.Where(p => p.Role == role);
            }
            if (!string.IsNullOrWhiteSpace(permissionType))
            {
                permissionToDeleteQuery.Where(p => p.Type == permissionType);
            }
            var permissionsToDelete = permissionToDeleteQuery.ToList();
            if (permissionsToDelete != null && permissionsToDelete.Count > 0)
            {
                var errorsBag = new ConcurrentBag<string>();
                var deletedIds = new ConcurrentBag<string>();
                var batch = new BatchRequest(driveService);
                BatchRequest.OnResponse<Permission> callback = delegate (
                Permission permission,
                RequestError error,
                int index,
                System.Net.Http.HttpResponseMessage message)
                {
                    if (error != null)
                    {
                        errorsBag.Add(error?.Message ?? "");
                    }
                    else
                    {
                        deletedIds.Add(permission?.Id ?? "");
                    }
                };
                PermissionsResource.DeleteRequest deleteRequest;
                foreach(var p in permissionsToDelete){
                    deleteRequest = driveService.Permissions.Delete(fileId, p.Id);
                    batch.Queue(deleteRequest, callback);
                }

                await batch.ExecuteAsync();
            }

            return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(true), googleCredential);
        }

        
        public async Task<bool> DeleteRootFolderAsync(string googleFolderId)
        {
            if (string.IsNullOrWhiteSpace(googleFolderId))
                throw new ArgumentNullException("googleFolderId");

            var rootFolder = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(
                filter: f => f.GoogleFileId == googleFolderId && f.FolderType == (int)FolderType.ROOT && f.PrentFolderId == null && !f.IsDeleted
                );

            _gradeBookRepository.Delete<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(rootFolder);

            return true;

        }

        public async Task<bool> DeleteInnerFolderAsync(string googleFolderId)
        {
            if (string.IsNullOrWhiteSpace(googleFolderId))
                throw new ArgumentNullException("googleFolderId");

            var innerFolder = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(
                filter: f => f.GoogleFileId == googleFolderId && f.FolderType == (int)FolderType.INNER && f.PrentFolderId != null && !f.IsDeleted
                );
            
            _gradeBookRepository.Delete<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(innerFolder);
            return true;
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