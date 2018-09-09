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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GdevApps.Portal.Controllers
{
    //[Authorize]
    [AllowAnonymous]
    public class TeacherController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

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

        [HttpGet]
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
                        Id = "2",
                        ClassroomSheets = Singleton.Instance.Sheets.Where(s => s.ClassroomId == "2").ToList()
                    },
                    new ClassesViewModel{
                        CourseWorksCount = 5,
                        StudentsCount = 10,
                        Name = "English",
                        Id = "1",
                        ClassroomSheets = Singleton.Instance.Sheets.Where(s => s.ClassroomId == "1").ToList()
                    },
                    new ClassesViewModel{
                        CourseWorksCount = 5,
                        Description = "This is the desciption for music",
                        StudentsCount = 5,
                        Name = "Music",
                        Id = "3",
                        ClassroomSheets = Singleton.Instance.Sheets.Where(s => s.ClassroomId == "3").ToList()
                    }
                };
                return Ok(new {data = classes});
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetStudents(string classId){
            try{
                var allStudents = new List<StudentsViewModel>(){
                    new StudentsViewModel{
                        Name = "Pasha",
                        Email = "pavlo.karasyuk@gmail.com",
                        ClassId = "1",
                        PrentEmails = new List<string>{"parentEmail@example.xyz","parentEmail2@example.xyz"},
                        Id = "1-1",
                        IsInClassroom = true
                    },
                     new StudentsViewModel{
                        Name = "Sasha",
                        Email = "Sasha@gmail.com",
                        ClassId = "1",
                        PrentEmails = new List<string>{"parentEmail@example.xyz","parentEmail2@example.xyz"},
                        Id = "1101",
                        IsInClassroom = false

                    },
                     new StudentsViewModel{
                        Name = "Dasha",
                        Email = "Dasha@gmail.com",
                        ClassId = "1",
                        PrentEmails = new List<string>{"parentEmail@example.xyz","parentEmail2@example.xyz"},
                        Id = "1102",
                        IsInClassroom = true
                    },
                     new StudentsViewModel{
                        Name = "Alex",
                        Email = "Alex@gmail.com",
                        ClassId = "2",
                        PrentEmails = new List<string>{"parentEmail@example.xyz","parentEmail2@example.xyz"},
                        Id = "1103",
                        IsInClassroom = false
                    },
                     new StudentsViewModel{
                        Name = "Pasha",
                        Email = "pavlo.karasyuk@gmail.com",
                        ClassId = "3",
                        PrentEmails = new List<string>{"parentEmail@example.xyz","parentEmail2@example.xyz"},
                        Id = "1104",
                        IsInClassroom = true
                    }
                };

                if(classId == "0")
                {
                    return Ok(new { data = allStudents });
                }

                var filteredStudents = allStudents.Where(s => s.ClassId == classId).ToList();
                return Ok(new {data = filteredStudents});
            }catch(Exception ex){
                 return BadRequest(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClassesForStudents()
        {
            try
            {
                List<ClassesViewModel> classes = new List<ClassesViewModel>
                {
                    new ClassesViewModel{
                        CourseWorksCount = 10,
                        Description = "This is the desciption",
                        StudentsCount = 5,
                        Name = "Math",
                        Id = "2",
                        ClassroomSheets = Singleton.Instance.Sheets.Where(s => s.ClassroomId == "2").ToList()
                    },
                    new ClassesViewModel{
                        CourseWorksCount = 5,
                        StudentsCount = 10,
                        Name = "English",
                        Id = "1",
                        ClassroomSheets = Singleton.Instance.Sheets.Where(s => s.ClassroomId == "1").ToList()
                    },
                    new ClassesViewModel{
                        CourseWorksCount = 5,
                        Description = "This is the desciption for music",
                        StudentsCount = 5,
                        Name = "Music",
                        Id = "3",
                        ClassroomSheets = Singleton.Instance.Sheets.Where(s => s.ClassroomId == "3").ToList()
                    }
                };

                var classesList = new SelectList(classes, "Id", "Name");

                return View("ClassesForStudents",new ClassesForStudentsViewModel{Classes = classesList});
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        public ActionResult AddGradebook(string classroomId)
        {
            if(!string.IsNullOrEmpty(classroomId)){
                return PartialView("_AddGradebook", new ClassSheetsViewModel() { ClassroomId = classroomId });
            }

            return BadRequest(classroomId);
        }

        [HttpPost]
        public ActionResult AddGradebook(ClassSheetsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sheet = Singleton.Instance.Sheets.Where(s => s.ClassroomId == model.ClassroomId && s.Id == model.Id).FirstOrDefault();
                if (sheet != null)
                {
                    ModelState.AddModelError("Id", $"Gradebook with suck id already exists");
                    return PartialView("_AddGradebook", model);
                }

                var isValidLink = CheckLink(model.Link);
                if (!isValidLink)
                {
                    ModelState.AddModelError("Link", $"Link is not valid. Provide a valid link");
                    return PartialView("_AddGradebook", model);
                }

                Singleton.Instance.Sheets.Add(model);
                return PartialView("_AddGradebook", model);
            }

            return PartialView("_AddGradebook", model);
        }

        [HttpPost]
        public ActionResult EditGradebook(ClassSheetsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sheet = Singleton.Instance.Sheets.Where(s => s.Id == model.Id).FirstOrDefault();
                if (sheet != null)
                {
                    var isValidLink = CheckLink(model.Link);
                    if(!isValidLink){
                        ModelState.AddModelError("Link", $"Link is not valid. Provide a valid link");
                        return PartialView("_EditGradebook", model);
                    }

                    sheet.Id = model.Id;
                    sheet.Link = model.Link;
                    sheet.Name = model.Name;
                    return Ok();
                }
                else
                {
                    return BadRequest($"Gradebook with id {model.Id} was not found");
                }

            }

            return PartialView("_EditGradebook", model);
        }

        [HttpGet]
        public ActionResult GetGradebookById(string classroomId, string gradebookId)
        {
            try
            {
                var sheet = Singleton.Instance.Sheets.Where(s => s.ClassroomId == classroomId && s.Id == gradebookId).FirstOrDefault();
                if (sheet != null)
                {
                    return PartialView("_EditGradebook", sheet);
                }
                else
                {
                    return BadRequest($"Gradebook with id {gradebookId} was not found");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult RemoveGradebook(string classroomId, string gradebookId){
            try
            {
                if (!string.IsNullOrEmpty(classroomId) && !string.IsNullOrEmpty(gradebookId))
                {
                    var sheet = Singleton.Instance.Sheets.Where(s => s.ClassroomId == classroomId && s.Id == gradebookId).FirstOrDefault();
                    if (sheet != null)
                    {
                        Singleton.Instance.Sheets.Remove(sheet);
                        return Ok();
                    }
                    else
                    {
                        return BadRequest($"Gradebook with id {gradebookId} was not found");
                    }
                }
                return BadRequest(classroomId);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
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

        private bool CheckLink(string link)
        {
            return link.StartsWith("https://docs.google.com");
        }
    }

    public sealed class Singleton
    {
        private static Singleton instance = null;
        private static readonly object padlock = new object();

        public readonly List<ClassSheetsViewModel> Sheets;

        Singleton()
        {
            Sheets = new List<ClassSheetsViewModel>();
        }

        public static Singleton Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Singleton();
                    }
                    return instance;
                }
            }
        }
    }
}