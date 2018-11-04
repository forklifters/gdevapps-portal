using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest;

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
        private const string _settingsRange = "Settings!A1:B25";
        private const string _statisticsRange = "Statistics!A1:N21";
        private const string _statisticsAverageMedianRange = "Statistics!G4:K4";
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
                            StudentName = studentName,
                            StudentId = email
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

                    string date = allValues[1][c]?.ToString() ?? "";
                    string creationDate = date;
                    if (string.IsNullOrEmpty(date))
                    {
                        creationDate = DateTime.UtcNow.ToString();
                    }
                    else
                    {
                        double dateDouble;
                        bool parseResult = Double.TryParse(date, out dateDouble);
                        date = parseResult ? DateTime.FromOADate(dateDouble).ToString() : "";
                        creationDate = date;
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
                        var percent = allValues[r][c+1]?.ToString() ?? "";//TODO:Check percent
                        double percentDouble;
                        double.TryParse(percent, out percentDouble);
                        studentSubmissions.Add(new GradebookStudentSubmission()
                        {
                            ClassId = classId,
                            Grade = grade?.ToString() ?? "",
                            CourseWorkId = c.ToString(),
                            StudentId = email,
                            StudentName = studentName,
                            Percent = percentDouble
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

            if (string.IsNullOrWhiteSpace(parentEmail))
                throw new ArgumentNullException("Parent email is null or mepty");

            var studentParent = student.Parents.Where(p => p.Email == parentEmail).FirstOrDefault();
            if (studentParent == null)
                throw new Exception("Student does not have such parent");

            try
            {
                var rootFolderIdResult = await _driveService.GetRootFolderIdAsync(googleCredential, refreshToken, userId);
                googleCredential = rootFolderIdResult.Credentials;

                //check if root folder still exists
                var isRootFolderStillExists = await _driveService.IsFileExistsAsync(rootFolderIdResult.ResultObject, googleCredential, refreshToken, userId);
                if (isRootFolderStillExists.ResultObject.Result == FileState.TRASHED)
                {
                    //Root folder exists in the Db but it was deleted from google drive. 
                    //Delete the root folder from the db with all inner folders
                    await _driveService.DeleteRootFolderAsync(rootFolderIdResult.ResultObject);
                    rootFolderIdResult = await _driveService.CreateRootFolderAsync(googleCredential, refreshToken, userId);
                }

                var oldParentGradeBook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(filter: f => f.Name == parentGradebookName);
                var isFileExistsResult = await _driveService.IsFileExistsAsync(oldParentGradeBook?.GoogleUniqueId ?? "", googleCredential, refreshToken, userId);
                googleCredential = isFileExistsResult.Credentials;

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = googleCredential,
                    ApplicationName = _configuration["ApplicationName"]
                });

                if (isFileExistsResult.Result == ResultType.SUCCESS && isFileExistsResult.ResultObject.Result == FileState.EXISTS)
                {
                    var getSpreadSheetRequest = service.Spreadsheets.Get(oldParentGradeBook.GoogleUniqueId);
                    var spreadSheet = await getSpreadSheetRequest.ExecuteAsync();
                    return await UpdateParentGradebookForStudentAsync(student, googleCredential, spreadSheet, false);
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
                        if (isFolderExists.ResultObject.Result == FileState.TRASHED)
                        {
                            //Inner folder exists in the Db but it was deleted from google drive. 
                            //Delete inner folder from db
                            await _driveService.DeleteInnerFolderAsync(innerFolderIdResult.ResultObject.GoogleFileId);
                            innerFolderIdResult = await _driveService.CreateInnerFolderAsync(rootFolderIdResult.ResultObject, student.Email, googleCredential, refreshToken, userId);
                            googleCredential = innerFolderIdResult.Credentials;
                        }
                        else if (isFolderExists.ResultObject.Result == FileState.NOTEXIST)
                        {
                            innerFolderIdResult = await _driveService.CreateInnerFolderAsync(rootFolderIdResult.ResultObject, student.Email, googleCredential, refreshToken, userId);
                            googleCredential = innerFolderIdResult.Credentials;
                        }
                    }
                    //TODO: Add attendance sheet use getStudentAttendanceInformation on gs file
                    var newSpreadSheet = await service.Spreadsheets.Create(new Spreadsheet()
                    {
                        Properties = new SpreadsheetProperties()
                        {
                            Title = parentGradebookName
                        },
                        Sheets = new List<Sheet>(){
                            new Sheet(){
                                Properties = new SheetProperties(){
                                    Index = 0,
                                    Title = "GradeBook"
                                }
                            },
                            new Sheet(){
                                Properties = new SheetProperties(){
                                    Index = 1,
                                    Title = "Settings"
                                }
                            },
                            new Sheet(){
                                Properties = new SheetProperties(){
                                    Index = 2,
                                    Title = "Statistics"
                                }
                            }
                        } 
                    }).ExecuteAsync();

                    //Get settings information and save on the settings sheet
                    var valueRenderOption = (SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum)1;
                    SpreadsheetsResource.ValuesResource.GetRequest requestSettings = service.Spreadsheets.Values.Get(mainGradeBook.GoogleUniqueId, _settingsRange);
                    requestSettings.ValueRenderOption = valueRenderOption;
                    Google.Apis.Sheets.v4.Data.ValueRange responseSettings = await requestSettings.ExecuteAsync();
                    var spreadSheetSettings = responseSettings.Values;
                    var valueRange = new ValueRange()
                    {
                        Range = _settingsRange,
                        MajorDimension = "ROWS",
                        Values = spreadSheetSettings
                    };
                    var updateRequest = service.Spreadsheets.Values.Update(valueRange, newSpreadSheet.SpreadsheetId, _settingsRange);
                    updateRequest.ValueInputOption = ValueInputOptionEnum.USERENTERED;
                    await updateRequest.ExecuteAsync();

                    //Get statistics information and save on the statistics sheet
                    SpreadsheetsResource.ValuesResource.GetRequest requestStatistics = service.Spreadsheets.Values.Get(mainGradeBook.GoogleUniqueId, _statisticsRange);
                    requestStatistics.ValueRenderOption = valueRenderOption;
                    var responseStatistics = await requestStatistics.ExecuteAsync();
                    var spreadSheetStatisticsValues = responseStatistics.Values;
                    var statisticsValueRange = new ValueRange()
                    {
                        Range = _statisticsRange,
                        MajorDimension = "ROWS",
                        Values = spreadSheetStatisticsValues
                    };

                    var updateStatisticsRequest = service.Spreadsheets.Values.Update(statisticsValueRange, newSpreadSheet.SpreadsheetId, _statisticsRange);
                    updateStatisticsRequest.ValueInputOption = ValueInputOptionEnum.USERENTERED;
                    await updateStatisticsRequest.ExecuteAsync();

                    //Update Gradebook sheet
                    await UpdateParentGradebookForStudentAsync(student, googleCredential, newSpreadSheet, true);

                    //TODO: uncomment.
                   // var moveResult = await _driveService.MoveFileToFolderAsync(newSpreadSheet.SpreadsheetId, innerFolderIdResult.ResultObject.GoogleFileId, googleCredential, refreshToken, userId);
                    //googleCredential = moveResult.Credentials;

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
            values[0] = new object[12]{
                        "Student name",
                        student.Name,
                        "Max points",
                        "Weight",
                        "Category",
                        "Term",
                        "Date",
                        "Grade",
                        "Percent",
                        "Final Grade",
                        "ClassroomId",
                        "Id"
                    };
            values[1] = new object[12]{
                        "Student email",
                        student.Email,
                        "",
                        "",
                        "",
                        "",
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
                var studentSubmission = student.Submissions.Where(s => s.CourseWorkId == cw.IdInGradeBook).FirstOrDefault();
                var array = new object[12]{
                            "Title",
                            cw.Title,
                            cw.MaxPoints,
                            cw.Weight,
                            cw.Category,
                            cw.Term,
                            cw.DueDate,
                            studentSubmission?.Grade,
                            studentSubmission?.Percent,//TODO:Check percent and final grade
                            student.FinalGrade,
                            cw.IdInClassroom,
                            cw.IdInGradeBook
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
                var folderIdResult = await _driveService.GetRootFolderIdAsync(googleCredential, refreshToken, userId);
                var rootFolderId = folderIdResult.ResultObject;

                var isFileExistsResult = await _driveService.IsFileExistsAsync(rootFolderId, googleCredential, refreshToken, userId);
                googleCredential = isFileExistsResult.Credentials;
                if (isFileExistsResult.ResultObject.Result == FileState.NOTEXIST)
                {
                    var rootFolderResult = await _driveService.CreateRootFolderAsync(googleCredential, refreshToken, userId);
                    googleCredential = rootFolderResult.Credentials;
                    rootFolderId = rootFolderResult.ResultObject;
                }
                else if (isFileExistsResult.ResultObject.Result == FileState.TRASHED)
                {
                    var deleteResult = await _driveService.DeleteRootFolderAsync(rootFolderId);
                    var rootFolderResult = await _driveService.CreateRootFolderAsync(googleCredential, refreshToken, userId);
                    googleCredential = rootFolderResult.Credentials;
                    rootFolderId = rootFolderResult.ResultObject;
                }

                var innerFolderName = $"{studentEmail}";
                var parentGradeBookName = $"{className} - {parentEmail} - {studentEmail}";
                var parent = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(filter: f => f.Email == parentEmail);
                if (parent == null)
                    throw new Exception($"Parent with email {parentEmail} was not found");

                var innerFolderResult = await _driveService.GetInnerFolderAsync(googleCredential, refreshToken, userId, rootFolderId, innerFolderName);
                //if inner folder does not exist create a new one
                if (innerFolderResult.Result != ResultType.SUCCESS || innerFolderResult.ResultObject == null)
                {
                    innerFolderResult = await _driveService.CreateInnerFolderAsync(rootFolderId, innerFolderName, googleCredential, refreshToken, userId);
                }
                else
                {
                    var innerFolderId = innerFolderResult.ResultObject.GoogleFileId;
                    //check if inner folder exists or trashed
                    var isInnerFolderExistsResult = await _driveService.IsFileExistsAsync(innerFolderId, googleCredential, refreshToken, userId);
                    googleCredential = isInnerFolderExistsResult.Credentials;
                    if (isInnerFolderExistsResult.ResultObject.Result == FileState.NOTEXIST)
                    {
                        innerFolderResult = await _driveService.CreateInnerFolderAsync(rootFolderId, innerFolderName, googleCredential, refreshToken, userId);
                    }
                    else if (isInnerFolderExistsResult.ResultObject.Result == FileState.TRASHED)
                    {
                        await _driveService.DeleteInnerFolderAsync(innerFolderId);
                        innerFolderResult = await _driveService.CreateInnerFolderAsync(rootFolderId, innerFolderName, googleCredential, refreshToken, userId);
                    }
                }

                googleCredential = innerFolderResult.Credentials;
                GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook parentGradeBook;
                if (!string.IsNullOrWhiteSpace(gradeBookId))
                {
                    //Get parent gradebook by gradebook id
                    parentGradeBook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(filter: f => f.GoogleUniqueId == gradeBookId);
                }
                else
                {
                    //get parentGradebook by name and main gradebook id
                    parentGradeBook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(filter: f => f.Name == parentGradeBookName && f.MainGradeBook.GoogleUniqueId == mainGradeBookId);
                }

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
                            FolderId = innerFolderResult.ResultObject.Id,
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
                            FolderId = innerFolderResult.ResultObject.Id,
                            SharedStatus = (int)FolderSharedStatus.SHARED,
                            ParentId = parent.Id
                        };

                        _gradeBookRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook>(parentSharedGradeBook);
                        _gradeBookRepository.Save();

                        //Set permissions
                        var permissionsResult = await _driveService.GrantPermission(googleCredential, refreshToken, userId, parentGradeBook.GoogleUniqueId, parentEmail, PermissionType.User, PermissionRole.Reader);
                        googleCredential = permissionsResult.Credentials;
                    }

                    //Add information to the Parent Student Table
                    //TODO: Create method to get only Id
                    var mainGradebook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(filter: g=> g.GoogleUniqueId == mainGradeBookId);
                    var parentStudent = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.ParentStudent>(
                        filter: s => s.ParentId == parent.Id && s.StudentEmail == studentEmail && s.GradeBookId == mainGradebook.Id
                        );

                    if (parentStudent == null)
                    {
                        var aspUser = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUsers>(filter: u=> u.Email == parentEmail);
                        parentStudent = new GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.ParentStudent(){
                            GradeBookId = mainGradebook.Id,
                            ParentAspId = aspUser.Id,
                            ParentId = parent.Id,
                            StudentEmail = studentEmail
                        };

                        _gradeBookRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.ParentStudent>(parentStudent);
                        _gradeBookRepository.Save();
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
            if (string.IsNullOrWhiteSpace(gradeBookId))
            {
                var parentGradeBook = await _gradeBookRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(
                    filter: p => p.MainGradeBook.GoogleUniqueId == mainGradeBookId && p.CreatedBy == userId && !p.IsDeleted
                    );
                if (parentGradeBook == null)
                {
                    return new TaskResult<BoolResult, ICredential>(ResultType.ERROR, new BoolResult(false), googleCredential, new List<string>(){
                    "Parent GradeBook was not found"
                    });
                }

                gradeBookId = parentGradeBook.GoogleUniqueId;

                //TODO: Remove when unshare or change the status? 
                //remove
                //TODO: Check why when deleting ParentGradeBook the ParentSharedGradeBook doe not delete
                _gradeBookRepository.Delete<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(parentGradeBook);

                var parentSharedGradeBookToDelete = await _gradeBookRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook>(
                filter: g => g.ParentGradeBook.GoogleUniqueId == gradeBookId && g.ParentId == parent.Id && g.SharedStatus == (int)SharedStatus.SHARED
                );
                if (parentSharedGradeBookToDelete != null)
                {
                    _gradeBookRepository.Delete<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook>(parentSharedGradeBookToDelete);

                }
                _gradeBookRepository.Save();
                return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(true), googleCredential);
            }

            //change the status
            var parentSharedGradeBook = await _gradeBookRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentSharedGradeBook>(
                filter: g => g.ParentGradeBook.GoogleUniqueId == gradeBookId && g.ParentId == parent.Id && g.SharedStatus == (int)SharedStatus.SHARED
                );
            if (parentSharedGradeBook == null)
            {
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


        public async Task<TaskResult<GradebookStudent, ICredential>> GetStudentInformationFromParentGradeBook(string externalAccessToken, string refreshToken, string userId, string gradeBookId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await GetStudentInformationFromParentGradeBook(googleCredential, refreshToken, userId, gradeBookId);
        }

        public async Task<TaskResult<GradebookStudent, ICredential>> GetStudentInformationFromParentGradeBook(ICredential googleCredential, string refreshToken, string userId, string gradeBookId)
        {
            var parentGradeBook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.ParentGradeBook>(filter: g => g.GoogleUniqueId == gradeBookId);
            if (parentGradeBook == null)
            {
                throw new ArgumentNullException("Parent Gradebook was not found");
            }

            parentGradeBook.MainGradeBook = await _gradeBookRepository.GetOneAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(filter: g => g.Id == parentGradeBook.MainGradeBookId);

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

            SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum valueRenderOption = (SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum)1;
            try
            {
                request = service.Spreadsheets.Values.Get(gradeBookId, _studentsRange);
                request.ValueRenderOption = valueRenderOption;
                response = request.Execute();
            }
            catch (Google.GoogleApiException ex)
            {
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.LogError(ex, $"An error occurred while retrieving students from GradeBook with id {gradeBookId}. Refreshing the token and trying again");
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

                        request = service.Spreadsheets.Values.Get(gradeBookId, _studentsRange);
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

            var gradeBookStudent = new GradebookStudent()
            {
                ClassName = parentGradeBook.ClassroomName,
                ClassId = parentGradeBook.MainGradeBook.ClassroomId
            };

            var submissions = new List<GradebookStudentSubmission>();
            var courseWorks = new List<GradebookCourseWork>();
            values = response.Values;
            for (var i = 0; i < values.Count; i++)
            {
                if (i == 0)
                {
                    gradeBookStudent.Name = values[0][1]?.ToString() ?? "";
                    continue;
                }
                else if (i == 1)
                {
                    gradeBookStudent.Email = values[1][1]?.ToString() ?? "";
                    continue;
                }
                else if (values[i].Count > 2)
                {

                    string date = values[i][6]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(date))
                    {
                        double dateDouble;
                        bool parseResult = Double.TryParse(date, out dateDouble);
                        date = parseResult ? DateTime.FromOADate(dateDouble).ToString() : "";
                    }

                    var courseWork = new GradebookCourseWork()
                    {
                        Title = values[i][1]?.ToString() ?? "",
                        MaxPoints = values[i][2]?.ToString() ?? "",
                        Weight = values[i][3]?.ToString() ?? "",
                        Category = values[i][4]?.ToString() ?? "",
                        Term = values[i][5]?.ToString() ?? "",
                        DueDate = date,
                        IdInClassroom = values[i][10]?.ToString() ?? "",
                        IdInGradeBook = values[i][11]?.ToString() ?? "",
                        
                    };

                    var percent = values[i][8]?.ToString() ?? "";
                    double percentDouble;
                    double.TryParse(percent, out percentDouble);
                    var grade = values[i][7]?.ToString() ?? "";
                    var submission = new GradebookStudentSubmission()
                    {
                        StudentId = gradeBookStudent.Email,
                        ClassId = gradeBookStudent.ClassId,
                        StudentName = gradeBookStudent.Name,
                        Grade = string.IsNullOrEmpty(grade) ? "No Mark" : grade,
                        CourseWorkId = courseWork.IdInGradeBook,
                        Percent = percentDouble
                    };

                    submissions.Add(submission);
                    courseWorks.Add(courseWork);
                    gradeBookStudent.FinalGrade = values[i][9]?.ToString() ?? "";
                }
            }

            gradeBookStudent.Submissions = submissions;
            gradeBookStudent.CourseWorks = courseWorks;

            return new TaskResult<GradebookStudent, ICredential>(ResultType.SUCCESS, gradeBookStudent, googleCredential);
        }

        
        public async Task<TaskResult<BoolResult, ICredential>> CreateSpreadsheet(ICredential googleCredential, string refreshToken, string userId)
        {
            try{
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = googleCredential,
                    ApplicationName = _configuration["ApplicationName"]
                });
				
				 var newSpreadSheet = await service.Spreadsheets.Create(new Spreadsheet()
                    {
                        Properties = new SpreadsheetProperties()
                        {
                            Title = "Test GradeBook"
                        },
                        Sheets = new List<Sheet>(){
                            new Sheet(){
                                Properties = new SheetProperties(){
                                    Index = 0,
                                    Title = "GradeBook"
                                }
                            },
                            new Sheet(){
                                Properties = new SheetProperties(){
                                    Index = 1,
                                    Title = "Settings"
                                }
                            },
                            new Sheet(){
                                Properties = new SheetProperties(){
                                    Index = 2,
                                    Title = "Statistics"
                                }
                            },
                        }   
                    }).ExecuteAsync();


                // var clearRequestBody = new ClearValuesRequest();
                // SpreadsheetsResource.ValuesResource.ClearRequest request = service.Spreadsheets.Values.Clear(clearRequestBody, newSpreadSheet.SpreadsheetId, _settingsRange);
                // var clearResponse = await request.ExecuteAsync();

                //TODO: get information from Statistics sheet G4 - course average and K4 - course median

                //var statistics = agradeBookCheck[3];
                // var courseAverage = statistics.getRange("G4").getValue();
                // if (!isNaN(courseAverage) && courseAverage != '') {
                //     courseAverage = courseAverage.toFixed(decimal) + "%";
                // }

                // var courseMedian = statistics.getRange("K4").getValue();
                // if (!isNaN(courseMedian) && courseMedian != '') {
                //     courseMedian = courseMedian.toFixed(decimal) + "%";
                // }



                SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum valueRenderOption = (SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum)1;
                var gradebookId = "1RUoDCarKOkr2I1iSs9hEuGUTny8kJuOKm-vnvFDFTLg";
                SpreadsheetsResource.ValuesResource.GetRequest requestSettings = service.Spreadsheets.Values.Get(gradebookId, _settingsRange);
                requestSettings.ValueRenderOption = valueRenderOption;
                
                Google.Apis.Sheets.v4.Data.ValueRange responseSettings = await requestSettings.ExecuteAsync();
                string className = "";
                string classId = "";
                var courseSettings = responseSettings.Values;
                var valueRange = new ValueRange()
                {
                    Range = _settingsRange,
                    MajorDimension = "ROWS",
                    Values = courseSettings
                };
                var updateRequest = service.Spreadsheets.Values.Update(valueRange, newSpreadSheet.SpreadsheetId, _settingsRange);
                updateRequest.ValueInputOption = ValueInputOptionEnum.USERENTERED;
                await updateRequest.ExecuteAsync();

                return new TaskResult<BoolResult, ICredential>(ResultType.SUCCESS, new BoolResult(true), googleCredential);
            }catch(Exception err){
                throw err;
            }
        }

        public async Task<TaskResult<BoolResult, ICredential>> CreateSpreadsheet(string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await CreateSpreadsheet(googleCredential, refreshToken, userId);
        }

        public GradebookStudentReport<StudentReport> GetStudentReportInformation(string externalAccessToken, string refreshToken, string userId, GradebookStudent student, GradebookSettings settings, string parentEmail){
            //Conditions for course works (cw):
            // if no cw has no title - skip
            // if weight == 0 - skip
            // round final grade Utilities.RoundNumber
            var isCategoryGradeBook = student.CourseWorks.Any(cw => !string.IsNullOrWhiteSpace(cw.Title));
            var isTermsGradeBook = student.CourseWorks.Any(cw => !string.IsNullOrWhiteSpace(cw.Term));
            var isSortedByCategory = settings.StudentReportSortBy == "Category";
            var isSortedByDateOrTitle = settings.StudentReportSortBy == "Date" || settings.StudentReportSortBy == "Title";
            var isSortedByTerm = settings.StudentReportSortBy == "Term";

             var studentGradeBookReport = new GradebookStudentReport<StudentReport>(){
                Student = student,
                FinalGrade = student.FinalGrade
            };

            //wrong combinations
            if(!isCategoryGradeBook && isSortedByCategory)
                throw new Exception("The non category GradeBook can not be sorted by Category");

            if(!isTermsGradeBook && isSortedByTerm)
                throw new Exception("The non term GradeBook can not be sorted by Term");

            if(isTermsGradeBook){
                if(isCategoryGradeBook){
                    throw new NotImplementedException();
                }else{
                    throw new NotImplementedException();
                }
            }else if(isCategoryGradeBook){
                 var categoriesInfos = GetCategorySortedReport(student, settings);
                 IEnumerable<StudentReport> studentReports = (IEnumerable<StudentReport>)categoriesInfos; //TODO:CHECK
                 studentGradeBookReport.ReportInfos = studentReports.ToList();
            }else{
                var standardInfo = GetStandardReport(student, settings);
                StudentReport studentReport = (StudentReport)standardInfo;
                studentGradeBookReport.ReportInfos = new List<StudentReport>(){studentReport};
            }

            return studentGradeBookReport;
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
        
        //TODO: Create a new table document properties which will contain information about the document settings and it should be connected to the main gradebook. 
        //TODO: As an option: the settings.gs script should be updated and settings should be saved into the db after click Save from UI. Or just make it on UI
        private GradebookReportInfo GetStandardReport(GradebookStudent student, GradebookSettings settings)
        {
            var reportInfo = new GradebookReportInfo(GradebookType.STANDARD);// TODO: Check if the same with total points
            foreach (var studentSubmission in student.Submissions)
            {
                if (studentSubmission.Grade?.ToUpper() == "E")
                {
                    reportInfo.StudentMark = "Excempted";
                }
                else if (!string.IsNullOrWhiteSpace(studentSubmission.Grade))
                {
                    reportInfo.IsGraded = true;
                    double studentGradeDouble = 0;
                    double courseWorkMaxPoints = 0;
                    if (studentSubmission.Grade.Contains('['))
                    {
                        var studentGrade = Utilities.GetNumberFromBrakets(studentSubmission.Grade);
                        if (studentGrade.Grade.HasValue && studentGrade.Total.HasValue)
                        {
                            studentGradeDouble = studentGrade.Grade.Value;
                            courseWorkMaxPoints = studentGrade.Total.Value;
                        }
                    }
                    else
                    {
                        Double.TryParse(studentSubmission.Grade, out studentGradeDouble);
                        var courseWorkMaxPointsStr = student.CourseWorks.Where(cw => cw.IdInGradeBook == studentSubmission.CourseWorkId).Select(cw => cw.MaxPoints).FirstOrDefault();
                        Double.TryParse(courseWorkMaxPointsStr, out courseWorkMaxPoints);
                    }

                    if (courseWorkMaxPoints > 0)//can not devide by 0. Avoid zeros
                    {
                        reportInfo.TotalPoints += courseWorkMaxPoints;
                        reportInfo.StudentMark = (studentGradeDouble / courseWorkMaxPoints).ToString();
                        reportInfo.Percent = (studentSubmission.Percent*100).ToString();
                        reportInfo.TotalMark += studentGradeDouble / courseWorkMaxPoints;
                    }
                }
            }

            return reportInfo;
        }

        private bool ConvertStringToBool(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
                return false;

            return value.ToUpper() == "YES" ? true : false;
        }
        private IEnumerable<GradebookStudentCategoryInfo> GetCategorySortedReport(GradebookStudent student, GradebookSettings settings)
        {
            //TODO: Finish and test
            var categories = student.CourseWorks.Where(cw => !string.IsNullOrWhiteSpace(cw.Category)).Select(cw => cw.Category).Distinct();
            var studentCategoryInfos = new List<GradebookStudentCategoryInfo>();

            foreach (var category in categories)
            {
                var studentCourseWorks = student.CourseWorks.Where(cw => cw.Category == category).ToList();
                var studentCourseWorksIds = studentCourseWorks.Select(cw => cw.IdInGradeBook).ToList();
                var studentSubmissions = student.Submissions.Where(s => studentCourseWorksIds.Contains(s.CourseWorkId)).ToList();
                var studentCategoryInfo = new GradebookStudentCategoryInfo(GradebookType.CATEGORY)
                {
                    AverageCatGrade = 0,
                    AverageStudentGrade = 0,
                    TotalWeight = 0,
                    StudentMark = "No Grade",
                    Percent = "-",
                    Category = category
                };

                var reportSubmissions = new List<GradebookReportSubmission>();

                foreach (var studentSubmission in studentSubmissions)
                {
                    if (studentSubmission.Grade?.ToUpper() == "E")
                    {
                        studentCategoryInfo.StudentMark = "Excempted";
                    }
                    else if (!string.IsNullOrWhiteSpace(studentSubmission.Grade) && studentSubmission.Grade != "No Mark")
                    {
                        studentCategoryInfo.IsGraded = true;
                        double studentGradeDouble = 0;
                        double courseWorkMaxPoints = 0;
                        double categoryWeight = 0;
                        if (studentSubmission.Grade.Contains('['))
                        {
                            var studentGrade = Utilities.GetNumberFromBrakets(studentSubmission.Grade);
                            if (studentGrade.Grade.HasValue && studentGrade.Total.HasValue)
                            {
                                var categoryWeightStr = student.CourseWorks.Where(cw => cw.IdInGradeBook == studentSubmission.CourseWorkId).Select(cw => cw.Weight).FirstOrDefault();
                                Double.TryParse(categoryWeightStr, out categoryWeight);
                                studentGradeDouble = studentGrade.Grade.Value;
                                courseWorkMaxPoints = studentGrade.Total.Value;
                            }
                        }
                        else
                        {
                            Double.TryParse(studentSubmission.Grade, out studentGradeDouble);
                            var courseWorkMaxPointsStr = student.CourseWorks.Where(cw => cw.IdInGradeBook == studentSubmission.CourseWorkId).Select(cw => cw.MaxPoints).FirstOrDefault();
                            Double.TryParse(courseWorkMaxPointsStr, out courseWorkMaxPoints);
                            var categoryWeightStr = student.CourseWorks.Where(cw => cw.IdInGradeBook == studentSubmission.CourseWorkId).Select(cw => cw.Weight).FirstOrDefault();
                            Double.TryParse(categoryWeightStr, out categoryWeight);
                        }

                        if (courseWorkMaxPoints > 0)//can not devide by 0. Avoid zeros
                        {
                            studentCategoryInfo.AverageStudentGrade += (studentGradeDouble * categoryWeight) / courseWorkMaxPoints;
                            studentCategoryInfo.TotalWeight += categoryWeight;
                            studentCategoryInfo.StudentMark = (studentGradeDouble / courseWorkMaxPoints).ToString();
                            studentCategoryInfo.Percent = (studentSubmission.Percent*100).ToString();
                        }
                    }

                                
                    var courseWork = student.CourseWorks.Where(cw => cw.IdInGradeBook == studentSubmission.CourseWorkId).FirstOrDefault();
                    reportSubmissions.Add(new  GradebookReportSubmission()
                    {
                        CourseWorkId = studentSubmission.CourseWorkId,
                        CreationTime = courseWork?.CreationTime,
                        DueDate = courseWork?.DueDate,
                        Grade = studentSubmission.Grade,
                        MaxPoints = courseWork?.MaxPoints,
                        Percent = studentSubmission.Percent,
                        Note = studentSubmission.Note,
                        StudentId =  studentSubmission.StudentId,
                        Title = courseWork?.Title
                    });
                }

                studentCategoryInfo.AverageCatGrade = Math.Round((studentCategoryInfo.AverageStudentGrade/studentCategoryInfo.TotalWeight)*100, settings.Decimal);
                studentCategoryInfo.Submissions = reportSubmissions;
                studentCategoryInfos.Add(studentCategoryInfo);
            }


            return studentCategoryInfos;
        }

        #endregion

        public async Task<TaskResult<GradebookSettings, ICredential>> GetSettingsFromParentGradeBookAsync(ICredential googleCredential, string refreshToken, string userId, string gradeBookId)
        {
            SheetsService service;
            SpreadsheetsResource.ValuesResource.GetRequest settingsRequest;
            Google.Apis.Sheets.v4.Data.ValueRange settingsResponse;
            IList<IList<object>> settingValues;
             service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"],
            });

            SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum valueRenderOption = (SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum)1;

            try
            {
                settingsRequest = service.Spreadsheets.Values.Get(gradeBookId, _settingsRange);
                settingsRequest.ValueRenderOption = valueRenderOption;
                settingsResponse = await settingsRequest.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.LogError(ex, $"An error occurred while retrieving settings from GradeBook with id {gradeBookId}. Token has expired. Refreshing the token and trying again");
                        var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse
                        {
                            RefreshToken = refreshToken
                        };;
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

                        settingsRequest = service.Spreadsheets.Values.Get(gradeBookId, _studentsRange);
                        settingsRequest.ValueRenderOption = valueRenderOption;
                        settingsResponse = settingsRequest.Execute();
                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        break;
                    default: throw ex;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving settings from GradeBook with id {gradeBookId}");
                throw ex;
            }

            settingValues = settingsResponse.Values;
            var statisticsRequest = service.Spreadsheets.Values.Get(gradeBookId, _statisticsAverageMedianRange);
            statisticsRequest.ValueRenderOption = valueRenderOption;
            var statisticsResponse = await statisticsRequest.ExecuteAsync();
            var statisticsValues = statisticsResponse.Values;
            
            var settings = new GradebookSettings();
            var settingsValues = settingsResponse.Values;
            var allSettings = new object[25][];//TODO: Check terms
            for (var a = 0; a < settingsValues.Count; a++)
            {
                var newArrey = new object[2];
                Array.Copy(settingsValues[a].ToArray(), newArrey, settingsValues[a].Count);
                allSettings[a] = newArrey;
            }

            // settings.CourseName = allSettings[0][0]?.ToString() ?? ""; 
            settings.CourseCode = allSettings[2][1]?.ToString() ?? "";
            settings.CourseName = allSettings[3][1]?.ToString() ?? "";
            settings.CoursePeriod = allSettings[4][1]?.ToString() ?? "";
            settings.TeacherName = allSettings[5][1]?.ToString() ?? "";
            settings.SchoolName = allSettings[6][1]?.ToString() ?? "";
            settings.SchoolPhone = allSettings[7][1]?.ToString() ?? "";

            int decimalPlaces;
            int.TryParse(allSettings[9][1]?.ToString() ?? "", out decimalPlaces);
            settings.Decimal = decimalPlaces;
            settings.Rounding = allSettings[10][1]?.ToString() ?? "";
            settings.ShowCourseAverage = ConvertStringToBool(allSettings[11][1]?.ToString());
            settings.ShowCourseMedian = ConvertStringToBool(allSettings[12][1]?.ToString());
            settings.SendEmailsAsNoReply = ConvertStringToBool(allSettings[13][1]?.ToString());
            settings.StudentReportSortBy = allSettings[15][1]?.ToString() ?? "";
            settings.ReportSchoolNameColor = allSettings[18][1]?.ToString() ?? "";
            settings.ReportHeadingColor = allSettings[19][1]?.ToString() ?? "";
            settings.ReportAlternatingColorFirst = allSettings[20][1]?.ToString() ?? "";
            settings.ReportAlternatingColorSecond = allSettings[21][1]?.ToString() ?? "";

            if (allSettings[24] != null)
            {
                int terms;
                int.TryParse(allSettings[24][1]?.ToString() ?? "", out terms);
                settings.Terms = terms;
            }

             if(statisticsValues != null)
            {
                double courseAverage;
                double courseMedian;
                double.TryParse(statisticsValues[0][0]?.ToString(), out courseAverage);
                double.TryParse(statisticsValues[0][4]?.ToString(), out courseMedian);

                settings.CourseAverage =  Math.Round(courseAverage, decimalPlaces);
                settings.CourseMedian = Math.Round(courseMedian, decimalPlaces);
            }

            return new TaskResult<GradebookSettings, ICredential>(ResultType.SUCCESS, settings, googleCredential);
        }

        public async Task<TaskResult<GradebookSettings, ICredential>> GetSettingsFromParentGradeBookAsync(string externalAccessToken, string refreshToken, string userId, string gradeBookId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await GetSettingsFromParentGradeBookAsync(googleCredential, refreshToken, userId, gradeBookId);
        }
    }
}