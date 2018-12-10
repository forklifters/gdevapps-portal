using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GdevApps.BLL.Contracts;
using GdevApps.BLL.Models;
using GdevApps.BLL.Models.GDevClassroomService;
using GdevApps.DAL.Repositories.GradeBookRepository;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace GdevApps.BLL.Domain
{
    public class GdevClassroomService : IGdevClassroomService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IAspNetUserService _aspUserService;
        private readonly IMapper _mapper;
        private readonly IGradeBookRepository _gradeBookRepository;


        public GdevClassroomService(
            IConfiguration configuration,
            ILogger logger,
            IAspNetUserService aspUserService,
            IMapper mapper,
            IGradeBookRepository gradeBookRepository
            )
        {
            _logger = logger;
            _configuration = configuration;
            _aspUserService = aspUserService;
            _mapper = mapper;
            _gradeBookRepository = gradeBookRepository;
        }

     
        public async Task<TaskResult<IEnumerable<GoogleClass>, ICredential>> GetAllClassesAsync(string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await GetAllClassesAsync(googleCredential, refreshToken, userId);
        }
        public Task<TaskResult<GoogleStudent, ICredential>> GetStudentByIdAsync(string studentId, string externalAccessToken, string refreshToken, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<TaskResult<IEnumerable<GoogleStudent>, ICredential>> GetStudentsByClassIdAsync(string externalAccessToken, string classId, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
           return await GetStudentsByClassIdAsync(googleCredential, classId, refreshToken, userId);
        }
        public async Task<TaskResult<GoogleClass, ICredential>> GetClassByIdAsync(string classroomId, string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await GetClassByIdAsync(classroomId, googleCredential, refreshToken, userId);
        }
        public async Task<TaskResult<IEnumerable<GoogleClass>, ICredential>>  GetAllClassesAsync(ICredential googleCredential, string refreshToken, string userId)
        {
            _logger.Debug("User {UserId} requested all classes", userId);
            CoursesResource.ListRequest request;
            ListCoursesResponse response;
            List<Course> courses = new List<Course>();
            ClassroomService service = new ClassroomService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"]
            }); ;
            try
            {
                request = service.Courses.List();
                request.PageSize = 100;
                request.TeacherId = "me";
                response = await request.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Error(ex, "An error occurred while retrieving courses from Google Classroom for user {UserId}", userId);
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.Debug(ex, "Token is expired. Refreshing the token and trying again");
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
                        service = new ClassroomService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = _configuration["ApplicationName"]
                        });
                        // Define request parameters.
                        request = service.Courses.List();
                        request.PageSize = 100;
                        response = await request.ExecuteAsync();

                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        break;
                    default: throw ex;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while retrieving courses from Google Classroom for user {UserId}", userId);
                throw ex;
            }
            var errorList = new List<string>();
            if(response.Courses == null){
                _logger.Debug("No courses have been found for this user {UserId}", userId);
                return new TaskResult<IEnumerable<GoogleClass>, ICredential>(ResultType.EMPTY, new List<GoogleClass>(), googleCredential, errorList);
            }

            courses.AddRange(response.Courses);
            request.PageToken = response.NextPageToken;
            while (!String.IsNullOrEmpty(request.PageToken))
            {
                try
                {
                    if (response.Courses != null && response.Courses.Count > 0)
                    {
                        response = await request.ExecuteAsync();
                        courses.AddRange(response.Courses);
                        request.PageToken = response.NextPageToken;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Error occurred while continued to retrieve courses for user {UserId}", userId);
                    request.PageToken = null;//stop the loop
                    errorList.Add(ex.Message);
                }
            }

            // List courses.
            var classes = new List<GoogleClass>();
            List<CourseWork> courseWorks = new List<CourseWork>();
            List<Student> students = new List<Student>();
            if (response.Courses != null && response.Courses.Count > 0)
            {
                foreach (var course in response.Courses)
                {
                    var courseWorksRequest = service.Courses.CourseWork.List(course.Id);
                    do
                    {
                        try
                        {
                            ListCourseWorkResponse cwList = await courseWorksRequest.ExecuteAsync();
                            courseWorks.AddRange(cwList.CourseWork);
                            courseWorksRequest.PageToken = cwList.NextPageToken;
                        }
                        catch (Exception ex)
                        {
                            _logger.Debug(ex, "Error occurred while retrieving courseWorks for user {UserId}. End cycle", userId);
                            courseWorksRequest.PageToken = null;
                            errorList.Add(ex.Message);
                        }

                    } while (!string.IsNullOrEmpty(courseWorksRequest.PageToken));

                    var studentsListRequest = service.Courses.Students.List(course.Id);
                    do
                    {
                        try
                        {
                            ListStudentsResponse studentList = await studentsListRequest.ExecuteAsync();
                            students.AddRange(studentList.Students);
                            studentsListRequest.PageToken = studentList.NextPageToken;
                        }
                        catch (Exception ex)
                        {
                            _logger.Debug(ex, "Error occured while retrieving students for user {UserId}", userId);
                            studentsListRequest.PageToken = null;
                            errorList.Add(ex.Message);
                        }
                    } while (!string.IsNullOrEmpty(studentsListRequest.PageToken));

                    var dalSheets = await _gradeBookRepository.GetAsync<GdevApps.DAL.DataModels.AspNetUsers.GradeBook.GradeBook>(filter: x=> x.ClassroomId == course.Id);
                    var sheets = _mapper.Map<List<GoogleClassSheet>>(dalSheets);

                    classes.Add(new GoogleClass
                    {
                        Name = course.Name,
                        Id = course.Id,
                        Description = course.Description,
                        CourseWorksCount = courseWorks.Count,
                        StudentsCount = students.Count,
                        ClassroomSheets = sheets
                    });
                }
            }

            _logger.Information("Classes were retrieved successfully for user {UserId}", userId);
            return new TaskResult<IEnumerable<GoogleClass>, ICredential>(ResultType.SUCCESS, classes, googleCredential, errorList);
        }
        public async Task<TaskResult<GoogleClass, ICredential>> GetClassByIdAsync(string classroomId, ICredential googleCredential, string refreshToken, string userId)
        {
            _logger.Debug("User {UserId} requested a class {ClassId} information", userId, classroomId);
            CoursesResource.GetRequest request;
            Course response;
            ClassroomService service = new ClassroomService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"]
            });

            try
            {
                request = service.Courses.Get(classroomId);
                response = await request.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Error(ex, "An error occurred while retrieving classroom {ClassId} from the Google Classroom", classroomId);
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.Error(ex, "Token is expired. Refreshing the token and trying again");
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
                        service = new ClassroomService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = _configuration["ApplicationName"]
                        });
                        request = service.Courses.Get(classroomId);
                        response = await request.ExecuteAsync();

                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        break;
                    default: throw ex;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error occurred while retrieving class with id {classroomId}");
                throw ex;
            }
            var classes = new List<GoogleClass>();
            List<CourseWork> courseWorks = new List<CourseWork>();
            List<Student> students = new List<Student>();
            var errorList = new List<string>();
            if (response != null)
            {
                var courseWorksRequest = service.Courses.CourseWork.List(response.Id);
                do
                {
                    try
                    {
                        ListCourseWorkResponse cwList = await courseWorksRequest.ExecuteAsync();
                        courseWorks.AddRange(cwList.CourseWork);
                        courseWorksRequest.PageToken = cwList.NextPageToken;
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex, "Error while retrieving courseWorks");
                        courseWorksRequest.PageToken = null;
                        errorList.Add(ex.Message);
                    }

                } while (!string.IsNullOrEmpty(courseWorksRequest.PageToken));

                var studentsListRequest = service.Courses.Students.List(response.Id);
                do
                {
                    try
                    {
                        ListStudentsResponse studentList = await studentsListRequest.ExecuteAsync();
                        students.AddRange(studentList.Students);
                        studentsListRequest.PageToken = studentList.NextPageToken;
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex, "Error while retrieving students");
                        studentsListRequest.PageToken = null;
                        errorList.Add(ex.Message);
                    }
                } while (!string.IsNullOrEmpty(studentsListRequest.PageToken));
                var googleClass = new GoogleClass
                {
                    Name = response.Name,
                    Id = response.Id,
                    Description = response.Description,
                    CourseWorksCount = courseWorks.Count,
                    StudentsCount = students.Count
                };

                _logger.Information("The class {ClassId} was successfully retrieved by user {UserId}", classroomId, userId);
                return new TaskResult<GoogleClass, ICredential>(ResultType.SUCCESS, googleClass, googleCredential, errorList);
            }

            _logger.Debug("The class {ClassId} was not found for user {UserId}", classroomId, userId);
            return new TaskResult<GoogleClass, ICredential>(ResultType.EMPTY, null, googleCredential, errorList);
        }
        public async Task<TaskResult<IEnumerable<GoogleStudent>, ICredential>> GetStudentsByClassIdAsync(ICredential googleCredential, string classId, string refreshToken, string userId)
        {
            _logger.Debug("User {UserId} requested all students for the class {ClassId}", userId, classId);
             ClassroomService service = new ClassroomService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleCredential,
                ApplicationName = _configuration["ApplicationName"]
            }); ;
            CoursesResource.GetRequest request;
            Google.Apis.Classroom.v1.CoursesResource.StudentsResource.ListRequest studentsListRequest;
            ListStudentsResponse studentList;

            request = service.Courses.Get(classId);
            studentsListRequest = service.Courses.Students.List(classId);
            try
            {
                studentList = await studentsListRequest.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                _logger.Error(ex, "An error occurred while retrieving students from Google Classroom {ClassId}.", classId);
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.Error(ex, "Token is expired. Refreshing the token and trying again");
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
                        service = new ClassroomService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = googleCredential,
                            ApplicationName = _configuration["ApplicationName"]
                        });

                        request = service.Courses.Get(classId);
                        studentsListRequest = service.Courses.Students.List(classId);
                        studentList = await studentsListRequest.ExecuteAsync();
                        await UpdateAllTokens(userId, googleCredential as UserCredential);
                        break;
                    default:
                        throw ex;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"An error occured while retrieving students for class: {classId}");
                throw ex;
            }

            //Get students from classroom
            var googleStudents = new List<GoogleStudent>();
            if (studentList != null && studentList.Students != null)
            {
                foreach (var student in studentList?.Students)
                {
                    googleStudents.Add(new GoogleStudent
                    {
                        ClassId = classId,
                        Email = student.Profile.EmailAddress,
                        Id = student.UserId,
                        IsInClassroom = true,
                        Name = student.Profile.Name.FullName
                    });
                }
            }

            _logger.Debug("{Number} students were successfully retrieved for class {ClassId}", googleStudents.Count, classId);
            return new TaskResult<IEnumerable<GoogleStudent>, ICredential>(ResultType.SUCCESS, googleStudents, googleCredential);
        }
        public async Task<TaskResult<GoogleStudent, ICredential>> GetStudentByIdAsync(string studentId, ICredential googleCredential, string refreshToken, string userId)
        {
            throw new NotImplementedException();
        }

        #region Private methods
        private async Task UpdateAllTokens(string userId, UserCredential credentials)
        {
            _logger.Debug("UpdateAllTokens was called for user {UserId}", userId);
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
        #endregion
    }
}