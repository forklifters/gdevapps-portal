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
using Microsoft.Extensions.Logging;

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
            ILogger<GdevClassroomService> logger,
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

        public bool AddGradebookAsync(GdevApps.BLL.Models.GDevClassroomService.GradeBook model)
        {
            try
            {
                var gradeBookModel = _mapper.Map<GdevApps.BLL.Models.GDevClassroomService.GradeBook>(model);
                _gradeBookRepository.Create<GdevApps.BLL.Models.GDevClassroomService.GradeBook>(gradeBookModel);

                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public Task<TaskResult<BoolResult, ICredential>> DeleteGradeBookAsync(string classroomId, string gradebookId)
        {
            throw new System.NotImplementedException();
        }
        public Task<TaskResult<BoolResult, ICredential>> EditGradebookAsync(GradeBook model)
        {
            throw new System.NotImplementedException();
        }
        public async Task<TaskResult<IEnumerable<GoogleClass>, ICredential>> GetAllClassesAsync(string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await GetAllClassesAsync(googleCredential, refreshToken, userId);
        }
        public async Task<GdevApps.BLL.Models.GDevClassroomService.GradeBook> GetGradebookByIdAsync(string gradebookId)
        {
           var dataModelGradebook = await _gradeBookRepository.GetByIdAsync<GdevApps.BLL.Models.GDevClassroomService.GradeBook>(gradebookId);
           var gradebook = _mapper.Map<GdevApps.BLL.Models.GDevClassroomService.GradeBook>(dataModelGradebook);
           return gradebook;
        }
        public Task<TaskResult<GoogleStudent, ICredential>> GetStudentByIdAsync(string studentId, string externalAccessToken, string refreshToken, string userId)
        {
            throw new NotImplementedException();
        }
        public async Task<TaskResult<IEnumerable<GoogleStudent>, ICredential>> GetStudentsByClassIdAndGradebookIdAsync(string externalAccessToken, string classId, string gradebookId, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
           return await GetStudentsByClassIdAndGradebookIdAsync(googleCredential, classId, gradebookId, refreshToken, userId);
        }
        public async Task<TaskResult<GoogleClass, ICredential>> GetClassByIdAsync(string classroomId, string externalAccessToken, string refreshToken, string userId)
        {
            ICredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
            return await GetClassByIdAsync(classroomId, googleCredential, refreshToken, userId);
        }
        public async Task<TaskResult<IEnumerable<GoogleClass>, ICredential>> GetAllClassesAsync(ICredential googleCredential, string refreshToken, string userId)
        {
            
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
                response = await request.ExecuteAsync();
            }
            catch (Google.GoogleApiException ex)
            {
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.LogError(ex, "An error occurred while retrieving courses from Google Classroom. Token is expired. Refreshing the token and trying again");
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
                _logger.LogError(ex, "Error occurred while retrieving classes");
                throw ex;
            }

            courses.AddRange(response.Courses);
            request.PageToken = response.NextPageToken;
            var errorList = new List<string>();
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
                    _logger.LogError(ex, "Error occurred while continued to retrieve classes");
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
                            _logger.LogError(ex, "Error while retrieving courseWorks");
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
                            _logger.LogError(ex, "Error while retrieving students");
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

            return new TaskResult<IEnumerable<GoogleClass>, ICredential>(ResultType.SUCCESS, classes, googleCredential, errorList);
        }
        public async Task<TaskResult<GoogleClass, ICredential>> GetClassByIdAsync(string classroomId, ICredential googleCredential, string refreshToken, string userId)
        {
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
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.LogError(ex, "An error occurred while retrieving courses from Google Classroom. Token is expired. Refreshing the token and trying again");
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
                _logger.LogError(ex, $"Error occurred while retrieving class with id {classroomId}");
                throw ex;
            }

            try
            {
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
                            _logger.LogError(ex, "Error while retrieving courseWorks");
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
                            _logger.LogError(ex, "Error while retrieving students");
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

                    return new TaskResult<GoogleClass, ICredential>(ResultType.SUCCESS, googleClass, googleCredential, errorList);
                }

                return new TaskResult<GoogleClass, ICredential>(ResultType.EMPTY, null, googleCredential, errorList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving courseworks or students");
                throw ex;
            }
        }
        public async Task<TaskResult<IEnumerable<GoogleStudent>, ICredential>> GetStudentsByClassIdAndGradebookIdAsync(ICredential googleCredential, string classId, string gradebookId, string refreshToken, string userId)
        {
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
                switch (ex?.Error?.Code)
                {
                    case 401:
                        _logger.LogError(ex, "An error occurred while retrieving students from Google Classroom. Refreshing the token and trying again");
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
                _logger.LogError(ex, $"An error occured while retrieving students by course id: {classId} and gradebook id {gradebookId}");
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
                        IsInClassroom = false,
                        Name = student.Profile.Name.FullName
                    });
                }
            }

            return new TaskResult<IEnumerable<GoogleStudent>, ICredential>(ResultType.SUCCESS, googleStudents, googleCredential);
        }
        public async Task<TaskResult<GoogleStudent, ICredential>> GetStudentByIdAsync(string studentId, ICredential googleCredential, string refreshToken, string userId)
        {
            throw new NotImplementedException();
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
                await _aspUserService.UpdateUserTokensAsync(tokenUpdatedTimeRecord);
            }
        }
        #endregion
    }
}