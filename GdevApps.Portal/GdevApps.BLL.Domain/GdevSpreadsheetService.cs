using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using GdevApps.BLL.Contracts;
using GdevApps.BLL.Models;
using GdevApps.BLL.Models.GDevClassroomService;
using GdevApps.BLL.Models.GDevDriveService;
using GdevApps.BLL.Models.GDevSpreadSheetService;
using GdevApps.DAL.Repositories.GradeBookRepository;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GdevApps.BLL.Domain
{
    public class GdevSpreadsheetService : IGdevSpreadsheetService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IAspNetUserService _aspUserService;
        private readonly IGdevDriveService _driveService;
        private readonly IGdevClassroomService _classroomService;
        private const string _studentsRange = "GradeBook";
        private const string _settingsCourseRange = "Settings!A48:A49";
        private readonly IGradeBookRepository _gradeBookRepository;
        private readonly IMapper _mapper;


        public GdevSpreadsheetService(
            IConfiguration configuration,
            ILogger<GdevClassroomService> logger,
            IAspNetUserService aspUserService,
            IGdevDriveService driveService,
            IGdevClassroomService classroomService,
            IGradeBookRepository gradeBookRepository,
            IMapper mapper
            )
        {
            _logger = logger;
            _configuration = configuration;
            _aspUserService = aspUserService;
            _driveService = driveService;
            _classroomService = classroomService;
            _gradeBookRepository = gradeBookRepository;
            _mapper = mapper;
        }

        public async Task<TaskResult<GradebookStudent, ICredential>> GetStudentByEmailFromGradebookAsync(string studentEmail, string externalAccessToken, string gradebookId, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await GetStudentByEmailFromGradebookAsync(studentEmail, googleCredential, gradebookId, refreshToken, userId);
        }
        public async Task<TaskResult<IEnumerable<GradebookStudent>, ICredential>> GetStudentsFromGradebookAsync(string externalAccessToken, string gradebookId, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await GetStudentsFromGradebookAsync(googleCredential, gradebookId, refreshToken, userId);
        }
        public async Task<TaskResult<BoolResult, ICredential>> IsGradeBookAsync(string gradebookId, string externalAccessToken, string refreshToken, string userId, string gradeBookLink = "")
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await IsGradeBookAsync(gradebookId, googleCredential, refreshToken, userId, gradebookId);
        }
        public async Task<TaskResult<string, ICredential>> SaveStudentIntoParentGradebookAsync(GradebookStudent student, string externalAccessToken, string refreshToken, string userId, string parentEmail, string parentGradebookName)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await SaveStudentIntoParentGradebookAsync(student, googleCredential, refreshToken, userId, parentEmail, parentGradebookName);
        }
        public async Task<TaskResult<BoolResult, ICredential>> IsGradeBookAsync(string gradebookId, ICredential googleCredential, string refreshToken, string userId, string gradeBookLink = "")
        {
            if (string.IsNullOrEmpty(gradebookId))
            {
                var regex = new Regex(@"/[-\w]{25,}/");
                var match = regex.Match(gradeBookLink);
                if (match.Success)
                {
                    gradebookId = match.Value.Replace("/", ""); ;
                }
            }

            SheetsService service;
            SpreadsheetsResource.GetRequest request;
            Google.Apis.Sheets.v4.Data.Spreadsheet response;
            // Create Classroom API service.
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"]
            });
            try
            {
                request = service.Spreadsheets.Get(gradebookId);
                response = await request.ExecuteAsync();
                if (response == null)
                {
                    return new TaskResult<BoolResult, ICredential>
                    {
                        Result = ResultType.SUCCESS,
                        ResultObject = new BoolResult(false),
                        Credentials = googleCredential
                    };
                }

                bool isGradebook = response.Sheets.Any(s => s.Properties.Title == "GradeBook") &&
                response.Sheets.Any(s => s.Properties.Title == "Settings") &&
                response.Sheets.Any(s => s.Properties.Title == "Statistics") &&
                response.Sheets.Any(s => s.Properties.Title == "Email Message");


                return new TaskResult<BoolResult, ICredential>
                {
                    Result = ResultType.SUCCESS,
                    ResultObject = new BoolResult(isGradebook),
                    Credentials = googleCredential
                };
            }
            catch (Google.GoogleApiException ex)
            {
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.LogError(ex, "An error occurred while retrieving courses from Google Classroom. Refreshing the token and trying again");
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
                        service = new SheetsService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = _configuration["ApplicationName"]
                        });
                        // Define request parameters.
                        request = service.Spreadsheets.Get(gradebookId);
                        response = await request.ExecuteAsync();

                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        if (response == null)
                        {
                            return new TaskResult<BoolResult, ICredential>
                            {
                                Result = ResultType.EMPTY,
                                ResultObject = new BoolResult(false),
                                Credentials = googleCredential
                            };
                        }

                        bool isGradebook = response.Sheets.Any(s => s.Properties.Title == "GradeBook") &&
                                            response.Sheets.Any(s => s.Properties.Title == "Settings") &&
                                            response.Sheets.Any(s => s.Properties.Title == "Statistics") &&
                                            response.Sheets.Any(s => s.Properties.Title == "Email Message");

                        return new TaskResult<BoolResult, ICredential>
                        {
                            Result = ResultType.SUCCESS,
                            ResultObject = new BoolResult(isGradebook),
                            Credentials = googleCredential
                        };
                    default: break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return new TaskResult<BoolResult, ICredential>
                {
                    Result = ResultType.ERROR,
                    ResultObject = new BoolResult(false),
                    Credentials = googleCredential
                };
            }

            return new TaskResult<BoolResult, ICredential>
            {
                Result = ResultType.EMPTY,
                ResultObject = new BoolResult(false),
                Credentials = googleCredential
            };
        }
        public async Task<TaskResult<IEnumerable<GradebookStudent>, ICredential>> GetStudentsFromGradebookAsync(ICredential googleCredential, string gradebookId, string refreshToken, string userId)
        {
            SheetsService service;
            SpreadsheetsResource.ValuesResource.GetRequest request;
            Google.Apis.Sheets.v4.Data.ValueRange response;
            List<GradebookStudent> gradedebookStudents = new List<GradebookStudent>();
            IList<IList<object>> values;
            // Create Classroom API service.
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"]
            });

            var studentIndex = 7;
            SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum valueRenderOption = (SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum)1;

            try
            {
                request = service.Spreadsheets.Values.Get(gradebookId, _studentsRange);
                request.ValueRenderOption = valueRenderOption;
                response = request.Execute();
            }
            catch (Google.GoogleApiException ex)
            {
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.LogError(ex, $"An error occurred while retrieving students from GradeBook with id {gradebookId}. Refreshing the token and trying again");
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

                        service = new SheetsService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = _configuration["ApplicationName"]
                        });

                        request = service.Spreadsheets.Values.Get(gradebookId, _studentsRange);
                        request.ValueRenderOption = valueRenderOption;
                        response = request.Execute();
                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        break;
                    default: throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //Get classId and name
            SpreadsheetsResource.ValuesResource.GetRequest requestSettings = service.Spreadsheets.Values.Get(gradebookId, _settingsCourseRange);
            requestSettings.ValueRenderOption = valueRenderOption;
            Google.Apis.Sheets.v4.Data.ValueRange responseSettings = await requestSettings.ExecuteAsync();
            string className = "";
            string classId = "";
            var courseSettings = responseSettings.Values;
            if (courseSettings != null && courseSettings.Count > 0)
            {
                className = courseSettings[0][0]?.ToString() ?? "";
                classId = courseSettings[1][0]?.ToString() ?? "";
            }

            values = response.Values;
            var subIndex = 13;
            int maxRows = 207;
            int maxCols = 313;
            try
            {
                //Get course works
                var allValues = new object[maxRows][];
                for (var a = 0; a < values.Count; a++)//MAX 207x313
                {
                    var newArrey = new object[maxCols];
                    Array.Copy(values[a].ToArray(), newArrey, values[a].Count);
                    allValues[a] = newArrey;
                }

                var courseWorks = new HashSet<GradebookCourseWork>();
                for (var c = subIndex; c < maxCols; c += 2)
                {
                    string title = allValues[0][c]?.ToString() ?? "";
                    if (allValues[0][c] == null || string.IsNullOrEmpty(title))
                    {
                        continue;
                    }

                    string date = allValues[1][c]?.ToString() ?? "";
                    string creationDate = date;
                    if (string.IsNullOrEmpty(date))
                    {
                        creationDate = DateTime.UtcNow.ToString();
                    }
                    string maxPoints = allValues[2][c]?.ToString() ?? "";
                    string weight = allValues[3][c]?.ToString() ?? "";
                    string category = allValues[4][c]?.ToString() ?? "";
                    string term = allValues[4][c + 1]?.ToString() ?? "";

                    courseWorks.Add(new GradebookCourseWork()
                    {
                        Title = title,
                        DueDate = date,
                        CreationTime = creationDate,
                        MaxPoints = maxPoints,
                        Weight = weight,
                        Category = category,
                        ClassId = classId,
                        IdInGradeBook = c.ToString(),
                        IdInClassroom = "",
                        Term = term
                    });
                }
                //Get students
                for (var r = studentIndex; r < values.Count; r++)
                {
                    var studentName = allValues[r][2]?.ToString() ?? "";
                    if (string.IsNullOrEmpty(studentName))
                    {
                        continue;
                    }

                    var email = allValues[r][5]?.ToString() ?? "";
                    var comment = allValues[r][9]?.ToString() ?? "";
                    var finalGrade = allValues[r][3]?.ToString() ?? "";
                    var photo = allValues[r][1]?.ToString() ?? "";
                    var parentEmail = allValues[r][7]?.ToString() ?? "";
                    var studentSubmissions = new List<GradebookStudentSubmission>();
                    //Get sunmissions
                    for (var c = subIndex; c < values[r].Count; c += 2)
                    {
                        var grade = allValues[r][c];
                        studentSubmissions.Add(new GradebookStudentSubmission()
                        {
                            ClassId = classId,
                            Grade = allValues[r][c]?.ToString() ?? "",
                            CourseWorkId = c.ToString(),
                            StudentId = studentName,
                            Email = email
                        });
                    }

                    gradedebookStudents.Add(new GradebookStudent
                    {
                        GradebookId = gradebookId,
                        Email = email,
                        Comment = comment,
                        FinalGrade = finalGrade,
                        Id = allValues[r][5]?.ToString() ?? "", //email as Id
                        Photo = photo,
                        Parents = parentEmail.Split(',').Select(p => new GradebookParent
                        {
                            Email = p
                        }).ToList(),
                        Name = studentName,
                        ClassId = classId,
                        ClassName = className,
                        Submissions = studentSubmissions,
                        CourseWorks = courseWorks
                    });
                }

                return new TaskResult<IEnumerable<GradebookStudent>, ICredential>
                {
                    Result = ResultType.SUCCESS,
                    ResultObject = gradedebookStudents,
                    Credentials = googleCredential
                };
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public async Task<TaskResult<GradebookStudent, ICredential>> GetStudentByEmailFromGradebookAsync(string studentEmail, ICredential googleCredential, string gradebookId, string refreshToken, string userId)
        {
            SheetsService service;
            SpreadsheetsResource.ValuesResource.GetRequest request;
            Google.Apis.Sheets.v4.Data.ValueRange response;
            IList<IList<object>> values;
            // Create Classroom API service.
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"]
            });

            var studentIndex = 7;
            SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum valueRenderOption = (SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum)1;

            try
            {
                request = service.Spreadsheets.Values.Get(gradebookId, _studentsRange);
                request.ValueRenderOption = valueRenderOption;
                response = await request.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.LogError(ex, $"An error occurred while retrieving students from GradeBook with id {gradebookId}. Token has expired. Refreshing the token and trying again");
                        var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse
                        {
                            RefreshToken = refreshToken
                        };
                        googleCredential = new UserCredential(new GoogleAuthorizationCodeFlow(
                           new GoogleAuthorizationCodeFlow.Initializer
                           {
                               ClientSecrets = new ClientSecrets
                               {
                                   ClientId = _configuration["installed:client_id"],
                                   ClientSecret = _configuration["installed:client_secret"]
                               }
                           }), "user", token);

                        service = new SheetsService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = _configuration["ApplicationName"]
                        });

                        request = service.Spreadsheets.Values.Get(gradebookId, _studentsRange);
                        request.ValueRenderOption = valueRenderOption;
                        response = request.Execute();
                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        break;
                    default: throw ex;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving students from GradeBook with id {gradebookId}");
                throw ex;
            }

            //Get classId and name
            SpreadsheetsResource.ValuesResource.GetRequest requestSettings = service.Spreadsheets.Values.Get(gradebookId, _settingsCourseRange);
            requestSettings.ValueRenderOption = valueRenderOption;
            Google.Apis.Sheets.v4.Data.ValueRange responseSettings = await requestSettings.ExecuteAsync();
            string className = "";
            string classId = "";
            var courseSettings = responseSettings.Values;
            if (courseSettings != null && courseSettings.Count > 0)
            {
                className = courseSettings[0][0]?.ToString();
                classId = courseSettings[1][0]?.ToString();
            }

            values = response.Values;
            var subIndex = 13;
            var maxValue = 313;
            try
            {
                //Get course works
                var allValues = new object[207][];
                for (var a = 0; a < values.Count; a++)
                {
                    var newArrey = new object[maxValue];
                    Array.Copy(values[a].ToArray(), newArrey, values[a].Count);
                    allValues[a] = newArrey;
                }

                var courseWorks = new HashSet<GradebookCourseWork>();
                for (var c = subIndex; c < maxValue; c += 2)
                {
                    string title = allValues[0][c]?.ToString();
                    if (allValues[0][c] == null || string.IsNullOrEmpty(title))
                    {
                        continue;
                    }

                    string date = allValues[1][c]?.ToString();
                    string creationDate = date;
                    if (string.IsNullOrEmpty(date))
                    {
                        creationDate = DateTime.UtcNow.ToString();
                    }
                    string maxPoints = allValues[2][c]?.ToString() ?? "";
                    string weight = allValues[3][c]?.ToString() ?? "";
                    string category = allValues[4][c]?.ToString() ?? "";
                    string term = allValues[4][c + 1]?.ToString() ?? "";

                    courseWorks.Add(new GradebookCourseWork()
                    {
                        Title = title,
                        DueDate = date,
                        CreationTime = creationDate,
                        MaxPoints = maxPoints,
                        Weight = weight,
                        Category = category,
                        ClassId = classId,
                        IdInGradeBook = c.ToString(),
                        IdInClassroom = "",
                        Term = term
                    });
                }
                //Get students
                for (var r = studentIndex; r < values.Count; r++)
                {
                    var studentName = allValues[r][2]?.ToString();
                    if (string.IsNullOrEmpty(studentName))
                    {
                        continue;
                    }

                    var email = allValues[r][5]?.ToString() ?? "";
                    if (email != studentEmail)
                    {
                        continue;
                    }

                    var comment = allValues[r][9]?.ToString() ?? "";
                    var finalGrade = allValues[r][3]?.ToString() ?? "";
                    var photo = allValues[r][1]?.ToString() ?? "";
                    var parentEmail = allValues[r][7]?.ToString() ?? "";
                    var studentSubmissions = new List<GradebookStudentSubmission>();
                    //Get sunmissions
                    for (var c = subIndex; c < values[r].Count; c += 2)
                    {
                        var grade = allValues[r][c];
                        studentSubmissions.Add(new GradebookStudentSubmission()
                        {
                            ClassId = classId,
                            Grade = grade?.ToString() ?? "",
                            CourseWorkId = c.ToString(),
                            StudentId = studentName,
                            Email = email
                        });
                    }

                    return new TaskResult<GradebookStudent, ICredential>
                    {
                        Result = ResultType.SUCCESS,
                        Credentials = googleCredential,
                        ResultObject = new GradebookStudent
                        {
                            GradebookId = gradebookId,
                            Email = email,
                            Comment = comment,
                            FinalGrade = finalGrade,
                            Id = email, //email as Id
                            Photo = photo,
                            Parents = parentEmail.Split(',').Select(p => new GradebookParent
                            {
                                Email = p
                            }).ToList(),
                            Name = studentName,
                            ClassId = classId,
                            ClassName = className,
                            Submissions = studentSubmissions,
                            CourseWorks = courseWorks
                        }

                    };
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return new TaskResult<GradebookStudent, ICredential>
            {
                Result = ResultType.EMPTY,
                Credentials = googleCredential
            };
        }
        public async Task<TaskResult<string, ICredential>> SaveStudentIntoParentGradebookAsync(GradebookStudent student, ICredential googleCredential, string refreshToken, string userId, string parentEmail, string parentGradebookName)
        {
            if (student == null)
                throw new ArgumentNullException("Student is null");
            
            if(string.IsNullOrWhiteSpace(parentEmail))
                throw new ArgumentNullException("Parent email is null or mepty");

            var studentParent = student.Parents.Where(p => p.Email == parentEmail).FirstOrDefault();
            if(studentParent == null)
                throw new Exception("Student does not have such parent");

            try
            {
                var rootFolderIdResult = await _driveService.GetRootFolderIdAsync(googleCredential, refreshToken, userId);
                googleCredential = rootFolderIdResult.Credentials;

                //check if root folder still exists
                var isRootFolderStillExists = await _driveService.IsFileExistsAsync(rootFolderIdResult.ResultObject, googleCredential, refreshToken, userId);
                if (isRootFolderStillExists.Result != ResultType.SUCCESS)
                {
                    //Root folder exists in the Db but it was deleted from google drive. 
                    //Delete the root folder from the db with all inner folders
                    await _driveService.DeleteRootFolderAsync(rootFolderIdResult.ResultObject);
                    rootFolderIdResult = await _driveService.CreateRootFolderAsync(googleCredential, refreshToken, userId);
                }

                var oldParentGradeBook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(filter: f => f.Name == parentGradebookName);
                var isFileExistsResult = await _driveService.IsFileExistsAsync(oldParentGradeBook?.Name ?? "", googleCredential, refreshToken, userId);
                googleCredential = isFileExistsResult.Credentials;

                 var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = googleCredential,
                        ApplicationName = _configuration["ApplicationName"]
                    });

                if (isFileExistsResult.Result == ResultType.SUCCESS && isFileExistsResult.ResultObject.Result)
                {
                    var getSpreadSheetRequest = service.Spreadsheets.Get(oldParentGradeBook.GoogleUniqueId);
                    var spreadSheet = await getSpreadSheetRequest.ExecuteAsync();
                    return await UpdateParentGradebookForStudentAsync(student, googleCredential, spreadSheet, true);
                }
                else
                {
                    var mainGradeBook = await GetGradebookByUniqueIdAsync(student.GradebookId);
                    var innerFolderName = $"{student.Email}";
                    var innerFolderIdResult = await _driveService.GetInnerFolderAsync(googleCredential, refreshToken, userId, rootFolderIdResult.ResultObject, innerFolderName);
                    googleCredential = innerFolderIdResult.Credentials;
                    if (innerFolderIdResult.Result != ResultType.SUCCESS || innerFolderIdResult.ResultObject == null)
                    {
                        innerFolderIdResult = await _driveService.CreateInnerFolderAsync(rootFolderIdResult.ResultObject, student.Email, googleCredential, refreshToken, userId);
                    }
                    else
                    {
                        var isFolderExists = await _driveService.IsFileExistsAsync(innerFolderIdResult.ResultObject.GoogleFileId, googleCredential, refreshToken, userId);
                        googleCredential = isFolderExists.Credentials;
                        if (isFolderExists.Result != ResultType.SUCCESS)
                        {
                            //Inner folder exists in the Db but it was deleted from google drive. 
                            //Delete inner folder from db
                            await _driveService.DeleteInnerFolderAsync(innerFolderIdResult.ResultObject.GoogleFileId);
                            innerFolderIdResult = await _driveService.CreateInnerFolderAsync(rootFolderIdResult.ResultObject, student.Email, googleCredential, refreshToken, userId);
                        }
                    }

                    var newSpreadSheet = await service.Spreadsheets.Create(new Spreadsheet()
                    {
                        Properties = new SpreadsheetProperties()
                        {
                            Title = parentGradebookName
                        }
                    }).ExecuteAsync();

                    BatchUpdateSpreadsheetRequest updateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest()
                    {
                        Requests = new List<Request>
                    {
                        new Request()
                        {
                            UpdateSheetProperties = new UpdateSheetPropertiesRequest(){
                                Properties = new SheetProperties(){
                                    Index = 0,
                                    Title = "GradeBook"
                                },
                                Fields = "title"
                            }
                        }
                    },
                        ResponseIncludeGridData = false,
                        IncludeSpreadsheetInResponse = false
                    };
                    await service.Spreadsheets.BatchUpdate(updateSpreadsheetRequest, newSpreadSheet.SpreadsheetId).ExecuteAsync();
                    var updateResult = await UpdateParentGradebookForStudentAsync(student, googleCredential, newSpreadSheet, true);
                    var moveResult = await _driveService.MoveFileToFolderAsync(newSpreadSheet.SpreadsheetId, innerFolderIdResult.ResultObject.GoogleFileId, googleCredential, refreshToken, userId);
                    googleCredential = moveResult.Credentials;

                    var parentGradeBook = new GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook
                    {
                        ClassroomName = student.ClassName,
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow,
                        GoogleUniqueId = newSpreadSheet.SpreadsheetId,
                        IsDeleted = false,
                        Link = newSpreadSheet.SpreadsheetUrl,
                        MainGradeBookId = mainGradeBook.Id,
                        Name = parentGradebookName
                    };

                    _gradeBookRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(parentGradeBook);
                    await _gradeBookRepository.SaveAsync();

                    return new TaskResult<string, ICredential>(ResultType.SUCCESS, newSpreadSheet.SpreadsheetId, googleCredential);
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public bool AddGradebook(GdevApps.BLL.Models.GDevClassroomService.GradeBook model)
        {
            try
            {
                var gradeBookModel = _mapper.Map<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(model);
                _gradeBookRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(gradeBookModel);
                _gradeBookRepository.Save();
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public async Task<bool> DeleteGradeBookAsync(string classroomId, string gradebookId)
        {
            try
            {
                var dataModelGradebook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(filter: f => f.GoogleUniqueId == gradebookId);
                if (dataModelGradebook != null)
                {
                    _gradeBookRepository.Delete(dataModelGradebook);
                    await _gradeBookRepository.SaveAsync();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> EditGradebookAsync(GradeBook model)
        {
            var gradebookToEdit = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(filter: g => g.GoogleUniqueId == model.GoogleUniqueId);
            gradebookToEdit.Name = model.Name;
            _gradeBookRepository.Update<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(gradebookToEdit);
            await _gradeBookRepository.SaveAsync();
            return true;
        }

        public async Task<GdevApps.BLL.Models.GDevClassroomService.GradeBook> GetGradebookByUniqueIdAsync(string gradebookId)
        {
            try
            {
                var dataModelGradebook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(filter: f => f.GoogleUniqueId == gradebookId);
                var gradebook = _mapper.Map<GdevApps.BLL.Models.GDevClassroomService.GradeBook>(dataModelGradebook);
                return gradebook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public GdevApps.BLL.Models.GDevClassroomService.GradeBook GetGradebookByIdAsync(int id)
        {
            try
            {
                var dataModelGradebook = _gradeBookRepository.GetById<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(id);
                var gradebook = _mapper.Map<GdevApps.BLL.Models.GDevClassroomService.GradeBook>(dataModelGradebook);
                return gradebook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<GradeBook>> GetGradeBooksByClassId(string classId)
        {
            var dataGradeBooks = await _gradeBookRepository.GetAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(filter: f => f.ClassroomId == classId);
            var gradeBooks = _mapper.Map<IEnumerable<GdevApps.BLL.Models.GDevClassroomService.GradeBook>>(dataGradeBooks);

            return gradeBooks;
        }

        public async Task<TaskResult<BoolResult, ICredential>> ShareGradeBook(
            string externalAccessToken,
            string refreshToken,
            string userId,
            string parentEmail,
            string studentEmail,
            string className,
            string gradeBookId,
            string mainGradeBookId)
        {

            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await ShareGradeBook(
                googleCredential,
                refreshToken,
                userId,
                parentEmail,
                studentEmail,
                className,
                gradeBookId,
                mainGradeBookId);
        }

        public async Task<TaskResult<string, ICredential>> UpdateParentGradebookForStudentAsync(
            GradebookStudent student,
            ICredential googleCredential,
            Spreadsheet spreadSheet, 
            bool isNew = false)
        {
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"]
            });

            string parentGradebookRange = "GradeBook";

            if (!isNew)
            {
                //clear all old values
                var clearRequestBody = new ClearValuesRequest();
                SpreadsheetsResource.ValuesResource.ClearRequest request = service.Spreadsheets.Values.Clear(clearRequestBody, spreadSheet.SpreadsheetId, parentGradebookRange);
                var clearResponse = await request.ExecuteAsync();
            }

            var parentGradeBookValuesRequest = await service.Spreadsheets.Values.Get(spreadSheet.SpreadsheetId, parentGradebookRange).ExecuteAsync();
            var numberOfCourseWorks = student.CourseWorks != null ? student.CourseWorks.Count() : 0;
            IList<IList<object>> values = new object[2 + numberOfCourseWorks][];
            values[0] = new object[8]{
                        "Student name",
                        student.Name,
                        "Max points",
                        "Weight",
                        "Category",
                        "Term",
                        "Date",
                        "Grade"
                    };
            values[1] = new object[8]{
                        "Student email",
                        student.Email,
                        "",
                        "",
                        "",
                        "",
                        "",
                        ""
                    };
            var index = 2;
            var studentCourseWorks = student.CourseWorks.ToList();
            foreach (var cw in studentCourseWorks)
            {
                var array = new object[8]{
                            "Title",
                            cw.Title,
                            cw.MaxPoints,
                            cw.Weight,
                            cw.Category,
                            cw.Term,
                            cw.DueDate,
                            student.Submissions.Where(s => s.CourseWorkId == cw.IdInGradeBook).Select(s => s.Grade).FirstOrDefault()
                        };
                values[index] = array;
                index++;
            }

            var valueRange = new ValueRange()
            {
                Range = parentGradebookRange,
                MajorDimension = "ROWS",
                Values = values
            };

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadSheet.SpreadsheetId, parentGradebookRange);
            updateRequest.ResponseValueRenderOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ResponseValueRenderOptionEnum.FORMULA;
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await updateRequest.ExecuteAsync();

            return new TaskResult<string, ICredential>(ResultType.SUCCESS, spreadSheet.SpreadsheetId, googleCredential);
        }

        public async Task<TaskResult<BoolResult, ICredential>> ShareGradeBook(
            ICredential googleCredential,
            string refreshToken,
            string userId,
            string parentEmail,
            string studentEmail,
            string className,
            string gradeBookId,
            string mainGradeBookId)
        {
            try
            {
                //Check the folder
                var folder = _gradeBookRepository.GetFirst<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.Folder>(filter: f => f.FolderType == (int)FolderType.ROOT && f.CreatedBy == userId);
                var rootFolderId = "";
                if (folder != null)
                {
                    var isFileExistsResult = await _driveService.IsFileExistsAsync(folder.GoogleFileId, googleCredential, refreshToken, userId);
                    googleCredential = isFileExistsResult.Credentials;

                    if (!isFileExistsResult.ResultObject.Result)
                    {
                        var rootFolderResult = await _driveService.CreateRootFolderAsync(googleCredential, refreshToken, userId);
                        googleCredential = rootFolderResult.Credentials;
                        rootFolderId = rootFolderResult.ResultObject;
                    }
                    else
                    {
                        rootFolderId = folder.GoogleFileId;
                    }
                }
                else
                {
                    var rootFolderResult = await _driveService.CreateRootFolderAsync(googleCredential, refreshToken, userId);
                    googleCredential = rootFolderResult.Credentials;
                    rootFolderId = rootFolderResult.ResultObject;
                }

                var innerFolderName = $"{studentEmail}";
                var parentGradeBookName = $"{className} - {parentEmail} - {studentEmail}";
                var parent = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(filter: f => f.Email == parentEmail);
                if (parent == null)
                    throw new Exception($"Parent with email {parentEmail} was not found");

                var parentFolderResult = await _driveService.GetInnerFolderAsync(googleCredential, refreshToken, userId, rootFolderId, innerFolderName);
                //if inner folder does not exist create a new one
                if (parentFolderResult.Result != ResultType.SUCCESS || parentFolderResult.ResultObject == null)
                {
                    parentFolderResult = await _driveService.CreateInnerFolderAsync(rootFolderId, innerFolderName, googleCredential, refreshToken, userId);
                }
                googleCredential = parentFolderResult.Credentials;

                //Get parent gradebook by gradebook id
                var parentGradeBook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(filter: f => f.GoogleUniqueId == gradeBookId);
                // no parent gradebook
                // a new one should be created
                if (parentGradeBook == null)
                {
                    var gradeBookStudentResult = await GetStudentByEmailFromGradebookAsync(studentEmail, googleCredential, mainGradeBookId, refreshToken, userId);
                    googleCredential = gradeBookStudentResult.Credentials;
                    if (gradeBookStudentResult.Result == ResultType.SUCCESS && gradeBookStudentResult.ResultObject != null)
                    {
                        GradebookStudent gradeBookStudent = gradeBookStudentResult.ResultObject;
                        var saveGradeBookResult = await SaveStudentIntoParentGradebookAsync(gradeBookStudent, googleCredential, refreshToken, userId, parentEmail, parentGradeBookName);
                        googleCredential = saveGradeBookResult.Credentials;
                        var newParentGradebookUniqueId = saveGradeBookResult.ResultObject;
                        var newParentGradebook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(filter: g => g.GoogleUniqueId == newParentGradebookUniqueId && !g.IsDeleted);
                        if (newParentGradebook == null)
                            throw new Exception($"Parent GradeBook with id {newParentGradebookUniqueId} was not found");

                        var parentSharedGradeBook = new GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook()
                        {
                            ParentGradeBookId = newParentGradebook.Id,
                            ParentAspId = parent.AspUserId,
                            TeacherAspId = userId,
                            FolderId = parentFolderResult.ResultObject.Id,
                            SharedStatus = (int)FolderSharedStatus.SHARED,
                            ParentId = parent.Id
                        };

                        _gradeBookRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook>(parentSharedGradeBook);
                        _gradeBookRepository.Save();

                        //Set permissions
                        var permissionsResult = await _driveService.GrantPermission(googleCredential, refreshToken, userId, newParentGradebookUniqueId, parentEmail, PermissionType.User, PermissionRole.Reader);
                        googleCredential = permissionsResult.Credentials;

                        return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(true), googleCredential);
                    }
                    else
                    {
                        throw new Exception($"Student with email {studentEmail} was not found in the GradeBook {mainGradeBookId}");
                    }
                }
                else
                {
                    //check if it is already shared
                    var sharedGradeBook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook>(filter: g => g.ParentGradeBookId == parentGradeBook.Id);
                    if (sharedGradeBook != null)
                    {
                        if (sharedGradeBook.SharedStatus == (int)FolderSharedStatus.SHARED)
                        {
                            //TODO: Return the right message: GradeBook has already been shared with parent
                            return new TaskResult<BoolResult, ICredential>(ResultType.ERROR, new BoolResult(false), googleCredential, new List<string>(){
                                $"GradeBook {parentGradeBook.GoogleUniqueId} has already been shared with parent {parentEmail}"
                            });
                        }
                        else
                        {
                            sharedGradeBook.SharedStatus = (int)FolderSharedStatus.SHARED;
                            _gradeBookRepository.Update<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook>(sharedGradeBook);
                            _gradeBookRepository.Save();

                            //Set permissions
                            var permissionsResult = await _driveService.GrantPermission(googleCredential, refreshToken, userId, parentGradeBook.GoogleUniqueId, parentEmail, PermissionType.User, PermissionRole.Reader);
                            googleCredential = permissionsResult.Credentials;
                        }
                    }
                    else
                    {
                        var parentSharedGradeBook = new GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook()
                        {
                            ParentGradeBookId = parentGradeBook.Id,
                            ParentAspId = parent.AspUserId,
                            TeacherAspId = userId,
                            FolderId = parentFolderResult.ResultObject.Id,
                            SharedStatus = (int)FolderSharedStatus.SHARED,
                            ParentId = parent.Id
                        };

                        _gradeBookRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook>(parentSharedGradeBook);
                        _gradeBookRepository.Save();

                        //Set permissions
                       var permissionsResult = await _driveService.GrantPermission(googleCredential, refreshToken, userId, parentGradeBook.GoogleUniqueId, parentEmail, PermissionType.User, PermissionRole.Reader);
                       googleCredential = permissionsResult.Credentials;
                    }
                }

                return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(true), googleCredential);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        
        public async Task<TaskResult<BoolResult, ICredential>> UnShareGradeBook(
            string externalAccessToken, 
            string refreshToken, 
            string userId, 
            string parentEmail, 
            string gradeBookId, 
            string mainGradeBookId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await UnShareGradeBook(
                googleCredential,
                refreshToken,
                userId,
                parentEmail,
                gradeBookId,
                mainGradeBookId
            );
        }

        public async Task<TaskResult<BoolResult, ICredential>> UnShareGradeBook(
            ICredential googleCredential, 
            string refreshToken, 
            string userId, 
            string parentEmail, 
            string gradeBookId, 
            string mainGradeBookId)
        {
            var parent = await _aspUserService.GetParentByEmailAsync(parentEmail);
            if(string.IsNullOrWhiteSpace(gradeBookId)){
                var parentGradeBook = await _gradeBookRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(
                    filter: p=> p.MainGradeBook.GoogleUniqueId == mainGradeBookId && p.CreatedBy == userId && !p.IsDeleted
                    );
                if (parentGradeBook == null)
                {
                    return new TaskResult<BoolResult, ICredential>(ResultType.ERROR, new BoolResult(false), googleCredential, new List<string>(){
                    "Parent GradeBook was not found"
                    });
                }

                gradeBookId = parentGradeBook.GoogleUniqueId;
            }

            var parentSharedGradeBook = await _gradeBookRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook>(
                filter: g => g.ParentGradeBook.GoogleUniqueId == gradeBookId && g.ParentId == parent.Id && g.SharedStatus == (int)SharedStatus.SHARED
                );
            if(parentSharedGradeBook == null){
                //TODO: Maybe return a message
                return new TaskResult<BoolResult, ICredential>(ResultType.ERROR, new BoolResult(false), googleCredential, new List<string>(){
                    "Nothing to unshare"
                }); 
            }
            parentSharedGradeBook.SharedStatus = (int)SharedStatus.NOTSHARED;
            _gradeBookRepository.Update<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook>(parentSharedGradeBook);
            await _gradeBookRepository.SaveAsync();
            //remove permissions
            var deleteResult = await _driveService.DeletePermissionAsync(googleCredential, refreshToken, userId, gradeBookId, PermissionType.User, PermissionRole.Reader);
            googleCredential = deleteResult.Result == ResultType.SUCCESS ? deleteResult.Credentials : googleCredential;
            return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(true), googleCredential);
        }


        #region Private methods
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