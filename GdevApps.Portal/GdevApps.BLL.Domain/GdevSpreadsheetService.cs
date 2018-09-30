using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GdevApps.BLL.Contracts;
using GdevApps.BLL.Models;
using GdevApps.BLL.Models.GDevClassroomService;
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

        public GdevSpreadsheetService(
            IConfiguration configuration,
            ILogger<GdevClassroomService> logger,
            IAspNetUserService aspUserService,
            IGdevDriveService driveService,
            IGdevClassroomService classroomService
            )
        {
            _logger = logger;
            _configuration = configuration;
            _aspUserService = aspUserService;
            _driveService = driveService;
            _classroomService = classroomService;
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
        public async Task<TaskResult<BoolResult, ICredential>> SaveStudentIntoParentGradebookAsync(GradebookStudent student, string parentGradebookId, string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await SaveStudentIntoParentGradebookAsync(student, parentGradebookId, googleCredential, refreshToken, userId);
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
                className = courseSettings[0][0]?.ToString();
                classId = courseSettings[1][0]?.ToString();
            }

            values = response.Values;
            var subIndex = 13;

            try
            {
                //Get course works
                var allValues = new object[207][];
                for (var a = 0; a < values.Count; a++)
                {
                    var newArrey = new object[213];
                    Array.Copy(values[a].ToArray(), newArrey, values[a].Count);
                    allValues[a] = newArrey;
                }

                var courseWorks = new HashSet<GradebookCourseWork>();
                for (var c = subIndex; c < 213; c += 2)
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
                    string maxPoints = allValues[2][c]?.ToString();
                    string weight = allValues[3][c]?.ToString();
                    string category = allValues[4][c]?.ToString();
                    string term = allValues[4][c + 1]?.ToString();

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

                    var email = allValues[r][5].ToString();
                    var comment = allValues[r][9].ToString();
                    var finalGrade = allValues[r][3].ToString();
                    var photo = allValues[r][1].ToString();
                    var parentEmail = allValues[r][7].ToString();
                    var studentSubmissions = new List<GradebookStudentSubmission>();
                    //Get sunmissions
                    for (var c = subIndex; c < values[r].Count; c += 2)
                    {
                        var grade = allValues[r][c];
                        studentSubmissions.Add(new GradebookStudentSubmission()
                        {
                            ClassId = classId,
                            Grade = grade != null ? grade.ToString() : "",
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
                        Id = allValues[r][5]?.ToString(), //email as Id
                        Photo = photo,
                        ParentEmails = parentEmail.Split(',').ToList<string>(),
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

            try
            {
                //Get course works
                var allValues = new object[207][];
                for (var a = 0; a < values.Count; a++)
                {
                    var newArrey = new object[213];
                    Array.Copy(values[a].ToArray(), newArrey, values[a].Count);
                    allValues[a] = newArrey;
                }

                var courseWorks = new HashSet<GradebookCourseWork>();
                for (var c = subIndex; c < 213; c += 2)
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
                    string maxPoints = allValues[2][c]?.ToString();
                    string weight = allValues[3][c]?.ToString();
                    string category = allValues[4][c]?.ToString();
                    string term = allValues[4][c + 1]?.ToString();

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

                    var email = allValues[r][5].ToString();
                    if (email != studentEmail)
                    {
                        continue;
                    }

                    var comment = allValues[r][9].ToString();
                    var finalGrade = allValues[r][3].ToString();
                    var photo = allValues[r][1].ToString();
                    var parentEmail = allValues[r][7].ToString();
                    var studentSubmissions = new List<GradebookStudentSubmission>();
                    //Get sunmissions
                    for (var c = subIndex; c < values[r].Count; c += 2)
                    {
                        var grade = allValues[r][c];
                        studentSubmissions.Add(new GradebookStudentSubmission()
                        {
                            ClassId = classId,
                            Grade = grade != null ? grade.ToString() : "",
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
                            Id = allValues[r][5]?.ToString(), //email as Id
                            Photo = photo,
                            ParentEmails = parentEmail.Split(',').ToList<string>(),
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
        public async Task<TaskResult<BoolResult, ICredential>> SaveStudentIntoParentGradebookAsync(GradebookStudent student, string parentGradebookId, ICredential googleCredential, string refreshToken, string userId)
        {
            if (student == null)
                return new TaskResult<BoolResult, ICredential>
                {
                    Result = ResultType.EMPTY,
                    ResultObject = new BoolResult(false)
                };
            try
            {

                var rootFolderIdResult = await _driveService.GetRootFolderIdAsync(googleCredential, refreshToken, userId);
                googleCredential = rootFolderIdResult.Credentials;
                var isFileExistsResult = await _driveService.IsFileExistsAsync(parentGradebookId, googleCredential, refreshToken, userId);
                googleCredential = isFileExistsResult.Credentials;

                if (isFileExistsResult.Result == ResultType.SUCCESS && isFileExistsResult.ResultObject.Result)
                {
                    //TEST

                }
                else
                {
                    var googleClassResult = await _classroomService.GetClassByIdAsync(student.ClassId, googleCredential, refreshToken, userId);
                    googleCredential = googleClassResult.Credentials;

                    string parentGradebookName = "";
                    if (googleClassResult.Result == ResultType.SUCCESS && googleClassResult.ResultObject == null)
                    {
                        var studentGradeBook = await _classroomService.GetGradebookByIdAsync(student.GradebookId);
                        parentGradebookName = $"{studentGradeBook.Name}_{student.Email}";
                    }
                    else
                    {
                        parentGradebookName = $"{googleClassResult.ResultObject.Name}_{student.Email}";
                    }

                    var innerFolderIdResult = await _driveService.CreateInnerFolderAsync(rootFolderIdResult.ResultObject, student.Email, googleCredential, refreshToken, userId);
                    googleCredential = innerFolderIdResult.Credentials;
                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = googleCredential,
                        ApplicationName = _configuration["ApplicationName"]
                    });

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

                    //get values
                    //TODO: Check the range
                    string parentGradebookRange = "GradeBook";
                    var parentGradeBookRequest = await service.Spreadsheets.Values.Get(newSpreadSheet.SpreadsheetId, parentGradebookRange).ExecuteAsync();
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

                    var updateRequest = service.Spreadsheets.Values.Update(valueRange, newSpreadSheet.SpreadsheetId, parentGradebookRange);
                    updateRequest.ResponseValueRenderOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ResponseValueRenderOptionEnum.FORMULA;
                    updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                    await updateRequest.ExecuteAsync();

                    var moveResult = await _driveService.MoveFileToFolderAsync(newSpreadSheet.SpreadsheetId, innerFolderIdResult.ResultObject, googleCredential, refreshToken, userId);

                    return moveResult;
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }

            throw new NotFiniteNumberException();
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