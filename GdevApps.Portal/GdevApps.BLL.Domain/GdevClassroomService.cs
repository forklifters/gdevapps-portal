using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public GdevClassroomService(
            IConfiguration configuration,
            ILogger<GdevClassroomService> logger
            )
        {
            _logger = logger;
            _configuration = configuration;
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

        public async Task<List<GoogleClass>> GetAllClassesAsync(string externalAccessToken, string refreshToken)
        {
            ClassroomService service;
            CoursesResource.ListRequest request;
            ListCoursesResponse response;
            try
            {
                //externalAccessToken +="1";
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
            }catch (Exception ex)
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

        public async Task<List<GoogleStudent>> GetStudentsByClassIdAndGradebookIdAsync(string externalAccessToken, string courseId, string gradebookId)
        {
            try
            {
                GoogleCredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
                
                // Create Classroom API service.
                var service = new ClassroomService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = googleCredential,
                    ApplicationName = _configuration["ApplicationName"]
                });

                CoursesResource.GetRequest request = service.Courses.Get(courseId);
                var studentsListRequest = service.Courses.Students.List(courseId);
                ListStudentsResponse studentList = await studentsListRequest.ExecuteAsync();
                var googleStudents = new List<GoogleStudent>();
                
                //Get students from classroom
                
                if (studentList != null && studentList.Students != null)
                {
                    foreach (var student in studentList?.Students)
                    {
                        googleStudents.Add(new GoogleStudent
                        {
                            ClassId = courseId,
                            Email = student.Profile.EmailAddress,
                            Id = student.UserId,
                            IsInClassroom = true,
                            Name = student.Profile.Name.FullName
                        });
                    }
                }

                //Get students from Gradebook
                if (!string.IsNullOrEmpty(gradebookId)){
                    //TODO
                }

                return googleStudents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured while retrieving students by course id: {courseId} and gradebook id {gradebookId}");
                return new List<GoogleStudent>();
            }
        }
    }
}