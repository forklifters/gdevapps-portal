using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GdevApps.Portal.Data;
using GdevApps.Portal.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using System.Collections.Generic;
using GdevApps.Portal.Models.TeacherViewModels;
using System;

namespace GdevApps.Portal.Controllers
{
    [Authorize]
    //[AllowAnonymous]
    public class TeacherController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        private readonly List<ClassSheetsViewModel> Contacts = new List<ClassSheetsViewModel>();

        public TeacherController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult ClassesAsync()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetClasses()
        {
            try
            {
               // List<ClassesViewModel> classes = await GetClassesAsync();
                List<ClassesViewModel> classes = new List<ClassesViewModel>
                {
                    new ClassesViewModel{
                        CourseWorksCount = 10,
                        Description = "This is the desciption",
                        StudentsCount = 5,
                        Name = "Math",
                        Id = "2"
                    },
                    new ClassesViewModel{
                        CourseWorksCount = 5,
                        StudentsCount = 10,
                        Name = "English",
                        Id = "1"
                    },
                    new ClassesViewModel{
                        CourseWorksCount = 5,
                        Description = "This is the desciption for music",
                        StudentsCount = 5,
                        Name = "Music",
                        Id = "3"
                    }
                };
                return Ok(new {data = classes});
            }
            catch (Exception err)
            {
                return BadRequest(err);
            }
        }


        [HttpGet]
        public ActionResult GetClassSheetInfo(int id)
        {
            var model = new ClassSheetsViewModel()
            {
                Name = "Pasha",
                Age = 25
            };
            return PartialView("_GetClassSheetInfo", model);
        }

        [HttpPost]
        public ActionResult GetClassSheetInfo(ClassSheetsViewModel model)
        {
            Contacts.Add(model);
            return PartialView("_GetClassSheetInfo", model);
        }

        private async Task<List<ClassesViewModel>> GetClassesAsync()
        {
            string externalAccessToken = null;
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (User.Identity.IsAuthenticated)
            {
                var userFromManager = await _userManager.GetUserAsync(User);
                string authenticationMethod = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationMethod)?.Value;
                if (authenticationMethod != null)
                {
                    externalAccessToken = await _userManager.GetAuthenticationTokenAsync(userFromManager,
                     authenticationMethod, "access_token");
                }
                else
                {
                    externalAccessToken = await _userManager.GetAuthenticationTokenAsync(userFromManager,
                     "Google", "access_token");
                }

                GoogleCredential googleCredential = GoogleCredential.FromAccessToken(externalAccessToken);
                // Create Classroom API service.
                var service = new ClassroomService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = googleCredential,
                    ApplicationName = _configuration["ApplicationName"],
                });

                // Define request parameters.
                CoursesResource.ListRequest request = service.Courses.List();
                request.PageSize = 100;

                try
                {
                    // List courses.
                    ListCoursesResponse response = request.Execute();
                    var classes = new List<ClassesViewModel>();
                    if (response.Courses != null && response.Courses.Count > 0)
                    {
                        foreach (var course in response.Courses)
                        {
                            var courseWorksRequest = service.Courses.CourseWork.List(course.Id);
                            ListCourseWorkResponse cwList = courseWorksRequest.Execute();

                            var studentsListRequest = service.Courses.Students.List(course.Id);
                            ListStudentsResponse studentList = studentsListRequest.Execute();

                            classes.Add(new ClassesViewModel
                            {
                                Name = course.Name,
                                Id = course.Id,
                                Description = course.Description ,
                                CourseWorksCount = cwList?.CourseWork?.Count,
                                StudentsCount = studentList?.Students?.Count
                            });
                        }
                    }

                    return classes;
                }
                catch (Exception err)
                {
                    return new List<ClassesViewModel>();
                }
            }

            return new List<ClassesViewModel>();
        }
    }
}