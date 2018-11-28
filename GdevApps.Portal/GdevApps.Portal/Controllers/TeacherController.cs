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
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using System.Collections.Generic;
using GdevApps.Portal.Models.TeacherViewModels;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using GdevApps.BLL.Contracts;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using GdevApps.BLL.Models.GDevClassroomService;
using System.Text.RegularExpressions;
using GdevApps.BLL.Models;
using GdevApps.Portal.Models.AccountViewModels;
using GdevApps.Portal.Attributes;
using GdevApps.Portal.Models.SharedViewModels;
using GdevApps.BLL.Models.AspNetUsers;
using GdevApps.BLL.Models.GDevSpreadSheetService;
using Serilog;

namespace GdevApps.Portal.Controllers
{
    [Authorize(Roles = UserRoles.Teacher)]
    [VerifyUserRole(UserRoles.Teacher)]
    public class TeacherController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IGdevClassroomService _classroomService;
        private readonly IMapper _mapper;
        private readonly HttpContext _context;
        private readonly IAspNetUserService _aspUserService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IGdevSpreadsheetService _spreadSheetService;
        private readonly IGdevDriveService _driveService;

        public TeacherController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger logger,
            IConfiguration configuration,
            IGdevClassroomService classroomService,
            IMapper mapper,
            IHttpContextAccessor httpContext,
            IAspNetUserService aspUserService,
            IGdevSpreadsheetService spreadSheetService,
            IGdevDriveService driveService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
            _classroomService = classroomService;
            _mapper = mapper;
            _context = httpContext.HttpContext;
            _aspUserService = aspUserService;
            _contextAccessor = httpContext;
            _spreadSheetService = spreadSheetService;
            _driveService = driveService;
        }



        [HttpGet]
        public async Task<IActionResult> GetGradebooksReport()
        {
            var userId = _userManager.GetUserId(User);
            var gradebooks = await _spreadSheetService.GetAllGradeBooksAsync(userId);
            var gradebooksNames = gradebooks.Select(g => new
            {
                Id = g.GoogleUniqueId,
                Name = g.Name
            }).ToList();
            var gradebooksNamesList = new SelectList(gradebooksNames, "Id", "Name");
            var studentReportsModel = new StudentGradebookReportsViewModel
            {
                Gradebooks = gradebooksNamesList,
                StudentReports = new List<ReportsViewModel>()
            };

            return View("StudentsReports", studentReportsModel);
        }

        [HttpGet]
        public async Task<IActionResult> Parents()
        {
            var userId = _userManager.GetUserId(User);
            var parents = await _aspUserService.GetAllParentsByTeacherAsync(userId);
            var parentsViewModel = new List<ParentStudentViewModel>();
            foreach (var p in parents)
            {
                var ps = new ParentStudentViewModel()
                {
                    Email = p.Email,
                    HasAccount = !string.IsNullOrWhiteSpace(p.AspUserId),
                };
                foreach (var gb in p.ParentSpreadsheets)
                {
                    ps.StudentEmail = gb.StudentEmail;

                    ps.MainGradeBookName = gb.MainGradeBookName;
                    ps.MainGradeBookNameUniqueId = gb.MainGradeBookGoogleUniqueId;
                    ps.MainGradeBookLink = gb.MainGradeBookLink;

                    ps.ParentGradebookName = gb.Name;
                    ps.ParentGradebookUniqueId = gb.GoogleUniqueId;
                    ps.ParentGradebookLink = gb.Link;
                }

                parentsViewModel.Add(ps);
            }
            return View(parentsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetReport(string mainGradeBookId, string studentEmail)
        {
            var userId = _userManager.GetUserId(User);
            var tokensInfo = await GetTokensInfoAsync();
            var externalAccessToken = GetAccessToken(tokensInfo);
            var refreshToken = GetRefreshToken(tokensInfo);
            var settings = await _spreadSheetService.GetSettingsFromParentGradeBookAsync(externalAccessToken, refreshToken, userId, mainGradeBookId);
            var student = await _spreadSheetService.GetStudentByEmailFromGradebookAsync(studentEmail, externalAccessToken, mainGradeBookId, refreshToken, userId);
             var user = await _userManager.GetUserAsync(User);

            var report = _spreadSheetService.GetStudentReportInformation(
                externalAccessToken,
                refreshToken,
                userId,
                student.ResultObject,
                settings.ResultObject);

            try
            {
                List<ReportsViewModel> studentReports = _mapper.Map<List<ReportsViewModel>>(report.ReportInfos);
                ReportSettings reportSettings = _mapper.Map<ReportSettings>(settings.ResultObject);
                var studentsModel = new StudentReportsViewModel
                {
                    StudentReports = studentReports,
                    ReportSettings = reportSettings,
                    StudentName = report.Student.Name,
                    FinalGrade = report.FinalGrade,
                    LetterGrade = report.FinalGradeLetter,
                    StudentGrade = $"{report.ReportInfos.First().TotalMark}%" 
                };
                return PartialView("_Reports", studentsModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public IActionResult RemoveParent(string parentEmail)
        {
            if(string.IsNullOrWhiteSpace(parentEmail))
            return BadRequest(parentEmail);

            //TODO: add service method to remove parent

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> GetGradebookStudents(string mainGradeBookId)
        {
            var userId = _userManager.GetUserId(User);
            var tokensInfo = await GetTokensInfoAsync();
            var externalAccessToken = GetAccessToken(tokensInfo);
            var refreshToken = GetRefreshToken(tokensInfo);
            var students = await _spreadSheetService.GetStudentsFromGradebookAsync(externalAccessToken, mainGradeBookId, refreshToken, userId);
            var studentEmails = students.ResultObject.Select(s => new StudentsViewModel
            {
                Id = s.Email,
                Name = s.Name ?? s.Email
            }).ToList();

            return Json(studentEmails);
        }

        public IActionResult Index()
        {
            return RedirectToAction("Classes");
        }

        [HttpGet]
        public async Task<IActionResult> Classes()
        {
            var classes = await GetAllClassesAsync();
            return View(classes);
        }

        [HttpPost]
        public async Task<IActionResult> GetStudents(string classId, string gradeBookId)
        {
            var userId = _userManager.GetUserId(User);
            var tokensInfo = await GetTokensInfoAsync();
            var externalAccessToken = GetAccessToken(tokensInfo);
            var refreshToken = GetRefreshToken(tokensInfo);
            var googleClassroomStudentResult = await _classroomService.GetStudentsByClassIdAsync(externalAccessToken,
                                                                                classId,
                                                                                refreshToken,
                                                                                userId);
            var googleStudents = googleClassroomStudentResult.ResultObject.ToList();
            //Get students from Gradebook
            if (!string.IsNullOrEmpty(gradeBookId))
            {
                var studentsTaskResult = await _spreadSheetService.GetStudentsFromGradebookAsync(googleClassroomStudentResult.Credentials,
                 gradeBookId,
                 refreshToken,
                 userId);
                var gradebookStudents = _mapper.Map<IEnumerable<GoogleStudent>>(studentsTaskResult.ResultObject);

                foreach (var student in gradebookStudents)
                {
                    var parents = student.Parents;
                    foreach (var p in parents)
                    {
                        var parentAccount = await _userManager.FindByEmailAsync(p.Email);
                        if (parentAccount != null)
                        {
                            p.HasAccount = true;
                            p.Name = parentAccount.UserName;
                        }
                        else
                        {
                            p.HasAccount = false;
                        }
                    }
                }

                var gradebookStudentsEmails = gradebookStudents.Select(s => s.Email).ToList();
                var googleStudentEmails = googleStudents.Where(s => gradebookStudents.Select(g => g.Email).Contains(s.Email)).ToList();
                foreach (var student in googleStudentEmails)
                {
                    student.IsInClassroom = true;
                }

                googleStudents.AddRange(gradebookStudents.Where(g => !googleStudents.Select(s => s.Email).Contains(g.Email)).ToList());
                //update parent emails
                foreach(var student in googleStudents)
                {
                    student.Parents = gradebookStudents.Where(s => s.Email == student.Email).FirstOrDefault()?.Parents;
                }
            }

            var studentsList = _mapper.Map<List<StudentsViewModel>>(googleStudents);

            return Ok(new { data = studentsList });
        }

        [HttpPost]
        public async Task<IActionResult> ShareGradeBook(string className, string parentEmail, string studentEmail, string mainGradeBookId)
        {
            var userId = _userManager.GetUserId(User);
            var tokensInfo = await GetTokensInfoAsync();
            var externalAccessToken = GetAccessToken(tokensInfo);
            var refreshToken = GetRefreshToken(tokensInfo);
            var result = await _spreadSheetService.ShareGradeBookAsync(
                externalAccessToken,
                refreshToken,
                userId,
                parentEmail,
                studentEmail,
                className,
                "",
                mainGradeBookId
            );

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UnshareGradeBook(string parentEmail, string mainGradeBookId)
        {
            var userId = _userManager.GetUserId(User);
            var tokensInfo = await GetTokensInfoAsync();
            var externalAccessToken = GetAccessToken(tokensInfo);
            var refreshToken = GetRefreshToken(tokensInfo);
            var result = await _spreadSheetService.UnShareGradeBookAsync(
                externalAccessToken,
                refreshToken,
                userId,
                parentEmail,
                "",
                mainGradeBookId
            );

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetGradeBooks(string classId)
        {
            var userId = _userManager.GetUserId(User);
            var gradebooks = await _spreadSheetService.GetGradeBooksByClassIdAsync(classId, userId);
            return Json(gradebooks.Select(g => new
            {
                UniqueId = g.GoogleUniqueId,
                Text = g.Name
            }));
        }

        [HttpGet]
        public async Task<IActionResult> GetClassesForStudents()
        {
            var classes = await GetAllClassesAsync();
            var classesList = new SelectList(classes, "Id", "Name");

            return View("ClassesForStudents", new ClassesForStudentsViewModel { Classes = classesList });
        }

        [HttpGet]
        public ActionResult AddGradebook(string classroomId)
        {
            if (!string.IsNullOrEmpty(classroomId))
            {
                return PartialView("_AddGradebook", new ClassSheetsViewModel() { ClassroomId = classroomId });
            }

            return BadRequest(classroomId);
        }

        [HttpPost]
        public async Task<ActionResult> AddGradebook(ClassSheetsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isValidLink = await CheckLink(model.Link);
                if (!isValidLink)
                {
                    ModelState.AddModelError("Link", $"Link is not valid. Provide a valid link");
                    return PartialView("_AddGradebook", model);
                }

                var sheet = await _spreadSheetService.GetGradebookByUniqueIdAsync(model.GoogleUniqueId);
                if (sheet != null)
                {
                    ModelState.AddModelError("Id", $"Gradebook with such id already exists");
                    return PartialView("_AddGradebook", model);
                }


                var gradeBookModel = new GradeBook
                {
                    ClassroomId = model.ClassroomId,
                    CreatedBy = _userManager.GetUserId(User),
                    CreatedDate = DateTime.UtcNow,
                    GoogleUniqueId = GetGradeBookIdFromLink(model.Link),
                    Name = model.Name,
                    Link = model.Link,
                    IsDeleted = false
                };

                var result = _spreadSheetService.AddGradebook(gradeBookModel);

                return PartialView("_AddGradebook", model);
            }

            return PartialView("_AddGradebook", model);
        }

        [HttpPost]
        public async Task<ActionResult> EditGradebook(ClassSheetsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isValidLink = await CheckLink(model.Link);
                if (!isValidLink)
                {
                    ModelState.AddModelError("Link", $"Link is not valid. Provide a valid link");
                    return PartialView("_EditGradebook", model);
                }

                var sheet = await _spreadSheetService.GetGradebookByUniqueIdAsync(model.GoogleUniqueId);
                if (sheet == null)
                {
                    ModelState.AddModelError("Id", $"Error occurred during the Gradebook update");
                    return PartialView("_EditGradebook", model);
                }

                GradeBook gradeBookModel = _mapper.Map<GradeBook>(model);
                var result = await _spreadSheetService.EditGradebookAsync(gradeBookModel);
                if (result)
                {
                    return PartialView("_EditGradebook", model);
                }
                else
                {
                    return BadRequest("Error occurred during the Gradebook update");
                }
            }

            return PartialView("_EditGradebook", model);
        }

        [HttpGet]
        public async Task<ActionResult> GetGradebookById(string classroomId, string gradebookId)
        {
            var gradeBookModel = await _spreadSheetService.GetGradebookByUniqueIdAsync(gradebookId);
            var gradeBook = _mapper.Map<ClassSheetsViewModel>(gradeBookModel);
            if (gradeBook != null)
            {
                return PartialView("_EditGradebook", gradeBook);
            }
            else
            {
                return BadRequest($"Gradebook with id {gradebookId} was not found");
            }
        }

        [HttpPost]
        public async Task<ActionResult> RemoveGradebook(string classroomId, string gradebookId)
        {
            if (!string.IsNullOrEmpty(classroomId) && !string.IsNullOrEmpty(gradebookId))
            {
                var result = await _spreadSheetService.DeleteGradeBookAsync(classroomId, gradebookId);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest($"Gradebook with id {gradebookId} was not found");
                }
            }
            return BadRequest(classroomId);
        }

        #region "Private"

        private async Task<List<ClassesViewModel>> GetAllClassesAsync()
        {
            var tokensInfo = await GetTokensInfoAsync();
            var externalAccessToken = GetAccessToken(tokensInfo); //await _context.GetTokenAsync("access_token"); 
            var refreshToken = GetRefreshToken(tokensInfo); //await _context.GetTokenAsync("refresh_token");
            var userId = _userManager.GetUserId(User);
            var serviceClassesResponse = await _classroomService.GetAllClassesAsync(externalAccessToken, refreshToken, userId);
            if (serviceClassesResponse.Result == ResultType.SUCCESS || serviceClassesResponse.Result == ResultType.EMPTY)
            {
                var classes = _mapper.Map<List<ClassesViewModel>>(serviceClassesResponse.ResultObject);
                return classes;
            }
            else
            {
                throw new Exception(string.Join(",", serviceClassesResponse.Errors));
            }
        }

        private async Task<bool> CheckLink(string link)
        {
            if (link.StartsWith("https://docs.google.com"))
            {
                var gradebookId = GetGradeBookIdFromLink(link);
                var tokensInfo = await GetTokensInfoAsync();
                var accessToken = GetAccessToken(tokensInfo);
                var refreshToken = GetRefreshToken(tokensInfo);
                var isGradeBookAsyncResult = await _spreadSheetService.IsGradeBookAsync(gradebookId, accessToken, refreshToken, _userManager.GetUserId(User), link);
                return isGradeBookAsyncResult.ResultObject.Result;
            }
            return false;
        }

        private async Task<TokenResponse> GetTokensInfoAsync()
        {
            var allTokens = await GetAllUserTokensAsync();
            var isUpdated = allTokens.Where(t => t.Name == "token_updated").Select(t => t.Value).FirstOrDefault();
            bool isUpdatedParsed;
            Boolean.TryParse(isUpdated, out isUpdatedParsed);
            if (!string.IsNullOrEmpty(isUpdated) && isUpdatedParsed)
            {
                DateTime createdDate;
                DateTime.TryParse(allTokens.Where(t => t.Name == "created").Select(t => t.Value).FirstOrDefault(), out createdDate);

                DateTime updatedDate;
                DateTime.TryParse(allTokens.Where(t => t.Name == "token_updated_time").Select(t => t.Value).FirstOrDefault(), out updatedDate);
                if (updatedDate > createdDate)
                {
                    return new TokenResponse(isUpdatedParsed, allTokens);
                }
            }
            return new TokenResponse(false, allTokens);
        }

        private string GetAccessToken(TokenResponse tokensInfo)
        {
            return tokensInfo.Tokens.Where(t => t.Name == "access_token").Select(t => t.Value).FirstOrDefault();
        }

        private string GetRefreshToken(TokenResponse tokensInfo)
        {
            return tokensInfo.Tokens.Where(t => t.Name == "refresh_token").Select(t => t.Value).FirstOrDefault();
        }

        private string GetTokenUpdatedTime(TokenResponse tokensInfo)
        {
            return tokensInfo.Tokens.Where(t => t.Name == "token_updated_time").Select(t => t.Value).FirstOrDefault();
        }

        private IEnumerable<GdevApps.BLL.Models.AspNetUsers.AspNetUserToken> GetAllUserTokens()
        {
            if (!User.Identity.IsAuthenticated)
            {
                RedirectToAction("logoutFromAttr", "Account");
                return null;
                //var aspUser = (IAspNetUserService)_context.RequestServices.GetService(typeof(IAspNetUserService));
            }

            return _aspUserService.GetAllTokensByUserId(_userManager.GetUserId(User));
        }

        private async Task<IEnumerable<GdevApps.BLL.Models.AspNetUsers.AspNetUserToken>> GetAllUserTokensAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                RedirectToAction("logoutFromAttr", "Account");
                return null;
                //var aspUser = (IAspNetUserService)_context.RequestServices.GetService(typeof(IAspNetUserService));
            }
            return await _aspUserService.GetAllTokensByUserIdAsync(_userManager.GetUserId(User));
        }

          private TokenResponse GetTokensInfo()
        {
            var allTokens = GetAllUserTokens();
            var isUpdated = allTokens.Where(t => t.Name == "token_updated").Select(t => t.Value).FirstOrDefault();
            bool isUpdatedParsed;
            Boolean.TryParse(isUpdated, out isUpdatedParsed);
            if (!string.IsNullOrEmpty(isUpdated) && isUpdatedParsed)
            {
                DateTime createdDate;
                DateTime.TryParse(allTokens.Where(t => t.Name == "created").Select(t => t.Value).FirstOrDefault(), out createdDate);

                DateTime updatedDate;
                DateTime.TryParse(allTokens.Where(t => t.Name == "token_updated_time").Select(t => t.Value).FirstOrDefault(), out updatedDate);
                if (updatedDate > createdDate)
                {
                    return new TokenResponse(isUpdatedParsed, allTokens);
                }
            }

            return new TokenResponse(false, allTokens);
        }

        private string GetGradeBookIdFromLink(string gradeBookLink)
        {
            var regex = new Regex(@"/[-\w]{25,}/");
            var match = regex.Match(gradeBookLink);
            if (match.Success)
            {
                return match.Value.Replace("/", ""); ;
            }
            else
            {
                return "";
            }
        }
        #endregion
    }
}