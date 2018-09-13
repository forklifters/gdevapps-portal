using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GdevApps.BLL.Contracts;
using GdevApps.BLL.Models.GDevClassroomService;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GdevApps.BLL.Domain
{
    public class GdevClassroomService : IGdevClassroomService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IGdevSpreadsheetService _gdevSpreadSheetService;

        private readonly IAspNetUserService _aspUserService;

        private readonly IMapper _mapper;

        public GdevClassroomService(
            IConfiguration configuration,
            ILogger<GdevClassroomService> logger,
            IGdevSpreadsheetService gdevSpreadSheetService,
            IAspNetUserService aspUserService,
            IMapper mapper
            )
        {
            _logger = logger;
            _configuration = configuration;
            _gdevSpreadSheetService = gdevSpreadSheetService;
            _aspUserService = aspUserService;
            _mapper = mapper;
        }

        public Task AddGradebookAsync(string classroomId)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteGradeBookAsync(string classroomId, string gradebookId)
        {
            throw new System.NotImplementedException();
        }

        public Task EditGradebookAsync(Gradebook model)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<GoogleClass>> GetAllClassesAsync(string externalAccessToken, string refreshToken, string userId)
        {
            ClassroomService service;
            CoursesResource.ListRequest request;
            ListCoursesResponse response;
            try
            {
                GoogleCredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
                // Create Classroom API service.
                service = new ClassroomService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = googleCredential,
                    ApplicationName = _configuration["ApplicationName"]
                });

                // Define request parameters.
                request = service.Courses.List();
                request.PageSize = 100;
                response = await request.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving courses from Google Classroom. Refreshing the token and trying again");
                var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse { RefreshToken = refreshToken };
                var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = _configuration["installed:client_id"],
                            ClientSecret = _configuration["installed:client_secret"]
                        }
                    }), "user", token);
                service = new ClassroomService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = _configuration["ApplicationName"]
                });
                // Define request parameters.
                request = service.Courses.List();
                request.PageSize = 100;
                response = await request.ExecuteAsync();

                await UpdateAllTokens(userId, credentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving classes");
                return new List<GoogleClass>();
            }

            try
            {
                // List courses.
                var classes = new List<GoogleClass>();
                if (response.Courses != null && response.Courses.Count > 0)
                {
                    foreach (var course in response.Courses)
                    {
                        var courseWorksRequest = service.Courses.CourseWork.List(course.Id);
                        ListCourseWorkResponse cwList = await courseWorksRequest.ExecuteAsync();

                        var studentsListRequest = service.Courses.Students.List(course.Id);
                        ListStudentsResponse studentList = await studentsListRequest.ExecuteAsync();

                        classes.Add(new GoogleClass
                        {
                            Name = course.Name,
                            Id = course.Id,
                            Description = course.Description,
                            CourseWorksCount = cwList?.CourseWork?.Count,
                            StudentsCount = studentList?.Students?.Count
                        });
                    }
                }

                return classes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving courseworks or students");
                return new List<GoogleClass>();
            }
        }

        private void GetNewAccessToken(Google.Apis.Services.BaseClientService service)
        {

        }

        public Task<Gradebook> GetGradebookByIdAsync(string classroomId, string gradebookId)
        {
            throw new System.NotImplementedException();
        }

        public Task<GoogleStudent> GetStudentById(string studentId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<GoogleStudent>> GetStudentsByClassIdAndGradebookIdAsync(string externalAccessToken, string courseId, string gradebookId, string refreshToken, string userId)
        {

            GoogleCredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            ClassroomService service;
            CoursesResource.GetRequest request;
            Google.Apis.Classroom.v1.CoursesResource.StudentsResource.ListRequest studentsListRequest;
            ListStudentsResponse studentList;
            service = new ClassroomService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"]
            });

            request = service.Courses.Get(courseId);
            studentsListRequest = service.Courses.Students.List(courseId);
            try
            {
                // Create Classroom API service.
                studentList = await studentsListRequest.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving students from Google Classroom. Refreshing the token and trying again");
                var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse { RefreshToken = refreshToken };
                var credentials = new UserCredential(new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = _configuration["installed:client_id"],
                            ClientSecret = _configuration["installed:client_secret"]
                        }
                    }), "user", token);
                service = new ClassroomService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = _configuration["ApplicationName"]
                });
                // Define request parameters.
                request = service.Courses.Get(courseId);
                studentsListRequest = service.Courses.Students.List(courseId);
                studentList = await studentsListRequest.ExecuteAsync();

                await UpdateAllTokens(userId, credentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured while retrieving students by course id: {courseId} and gradebook id {gradebookId}");
                return new List<GoogleStudent>();
            }

            //Get students from classroom
            var googleStudents = new List<GoogleStudent>();
            if (studentList != null && studentList.Students != null)
            {
                foreach (var student in studentList?.Students)
                {
                    googleStudents.Add(new GoogleStudent
                    {
                        ClassId = courseId,
                        Email = student.Profile.EmailAddress,
                        Id = student.UserId,
                        IsInClassroom = false,
                        Name = student.Profile.Name.FullName
                    });
                }
            }

            //Get students from Gradebook
            if (!string.IsNullOrEmpty(gradebookId))
            {
                var gradebookStudents = _mapper.Map<IEnumerable<GoogleStudent>>(await _gdevSpreadSheetService.GetStudentsFromGradebook(externalAccessToken, gradebookId, refreshToken, userId));
                var gradebookStudentsEmails = gradebookStudents.Select(s => s.Email).ToList();
                foreach (var student in googleStudents.Where(s => gradebookStudents.Select(g => g.Email).Contains(s.Email)).ToList())
                {
                    student.IsInClassroom = true;
                }
                googleStudents.AddRange(gradebookStudents.Where(g => !googleStudents.Select(s => s.Email).Contains(g.Email)).ToList());
            }

            return googleStudents;
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
                await _aspUserService.UpdateUserTokensAsync(tokenUpdatedTimeRecord);
            }
        }
    }
}