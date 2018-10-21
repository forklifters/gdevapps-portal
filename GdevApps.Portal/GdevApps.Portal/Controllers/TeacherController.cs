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
using GdevApps.BLL.Contracts;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using GdevApps.BLL.Models.GDevClassroomService;
using System.Text.RegularExpressions;
using GdevApps.BLL.Models;
using GdevApps.Portal.Models.AccountViewModels;
using GdevApps.Portal.Attributes;

namespace GdevApps.Portal.Controllers
{
    [Authorize(Roles = UserRoles.Teacher)]
    //[VerifyUserRole(UserRoles.Teacher)]
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

        private Singleton Singleton;

        private readonly IAspNetUserService _aspUserService;

        private readonly IHttpContextAccessor _contextAccessor;

        private readonly IGdevSpreadsheetService _spreadSheetService;

        private readonly IGdevDriveService _driveService;

        public TeacherController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<TeacherController> logger,
            IConfiguration configuration,
            IGdevClassroomService classroomService,
            IMapper mapper,
            IHttpContextAccessor httpContext,
            IAspNetUserService aspUserService,
            IHttpContextAccessor contextAccessor,
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
            _contextAccessor = contextAccessor;
            _spreadSheetService = spreadSheetService;
            _driveService = driveService;
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var gradeBookLink = "https://docs.google.com/spreadsheets/d/1RUoDCarKOkr2I1iSs9hEuGUTny8kJuOKm-vnvFDFTLg/edit?usp=drive_web&ouid=106890447120707259670";
            var gradebookId = "1RUoDCarKOkr2I1iSs9hEuGUTny8kJuOKm-vnvFDFTLg";
            //var gradebookId = "";
            var fileId = "1JYB-P0mcfYhJeKqG4_U22JaHj2L4MJURair1qkbe1sI";
            var folderId = "1QJY6E4Yww5y238DZgZ9drrSoJH0fgj2h";
            var userId = _userManager.GetUserId(User);

            var resul = await _driveService.MoveFileToFolderAsync(fileId, folderId, await GetAccessTokenAsync(), await GetRefreshTokenAsync(), userId);

            return Ok();
        }

        public IActionResult Index()
        {
            return RedirectToAction("ClassesAsync");
        }

        public IActionResult ClassesAsync()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            List<ClassesViewModel> classes = await GetClassesAsync();
            return Ok(new { data = classes });
        }

        [HttpPost]
        public async Task<IActionResult> GetStudents(string classId, string gradeBookId)
        {
            var userId = _userManager.GetUserId(User);
            var googleClassroomStudentResult = await _classroomService.GetStudentsByClassIdAsync(await GetAccessTokenAsync(),
                                                                                classId,
                                                                                await GetRefreshTokenAsync(),
                                                                                userId);
            var googleStudents = googleClassroomStudentResult.ResultObject.ToList();
            //Get students from Gradebook
            if (!string.IsNullOrEmpty(gradeBookId))
            {
                var studentsTaskResult = await _spreadSheetService.GetStudentsFromGradebookAsync(googleClassroomStudentResult.Credentials,
                 gradeBookId,
                  await GetRefreshTokenAsync(),
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
                foreach (var student in googleStudents.Where(s => gradebookStudents.Select(g => g.Email).Contains(s.Email)).ToList())
                {
                    student.IsInClassroom = true;
                }

                googleStudents.AddRange(gradebookStudents.Where(g => !googleStudents.Select(s => s.Email).Contains(g.Email)).ToList());
            }

            var studentsList = _mapper.Map<List<StudentsViewModel>>(googleStudents);

            return Ok(new { data = studentsList });
        }

        [HttpPost]
        public async Task<IActionResult> ShareGradeBook(string className, string parentEmail, string studentEmail, string mainGradeBookId)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _spreadSheetService.ShareGradeBook(
                await GetAccessTokenAsync(),
                await GetRefreshTokenAsync(),
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
            var result = await _spreadSheetService.UnShareGradeBook(
                await GetAccessTokenAsync(),
                await GetRefreshTokenAsync(),
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
            var gradebooks = await _spreadSheetService.GetGradeBooksByClassId(classId);
            return Json(gradebooks.Select(g => new
            {
                UniqueId = g.GoogleUniqueId,
                Text = g.Name
            }));
        }


        [HttpGet]
        public async Task<IActionResult> GetClassesForStudents()
        {
            var classes = await GetClassesAsync();
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

        private async Task<List<ClassesViewModel>> GetClassesAsync()
        {
            var serviceClassesRespnse = await _classroomService.GetAllClassesAsync(await GetAccessTokenAsync(), await GetRefreshTokenAsync(), _userManager.GetUserId(User));
            if (serviceClassesRespnse.Result == ResultType.SUCCESS || serviceClassesRespnse.Result == ResultType.EMPTY)
            {
                var classes = _mapper.Map<List<ClassesViewModel>>(serviceClassesRespnse.ResultObject);
                return classes;
            }
            else
            {
                throw new Exception(string.Join(",", serviceClassesRespnse.Errors));
            }
        }

        private async Task<bool> CheckLink(string link)
        {
            if (link.StartsWith("https://docs.google.com"))
            {
                var gradebookId = GetGradeBookIdFromLink(link);
                var isGradeBookAsyncResult = await _spreadSheetService.IsGradeBookAsync(gradebookId, await GetAccessTokenAsync(), await GetRefreshTokenAsync(), _userManager.GetUserId(User), link);
                return isGradeBookAsyncResult.ResultObject.Result;
            }
            return false;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            // Include the access token in the properties
            var accessToken = await _context.GetTokenAsync("access_token");
            var tokensInfo = await GetTokensInfoAsync();
            if (string.IsNullOrEmpty(accessToken) || tokensInfo.IsUpdated)
            {
                return tokensInfo.Tokens.Where(t => t.Name == "access_token").Select(t => t.Value).FirstOrDefault();
            }

            return accessToken;
        }

        private async Task<string> GetRefreshTokenAsync()
        {
            // Include the access token in the properties
            var refreshToken = await _context.GetTokenAsync("refresh_token");
            var tokensInfo = await GetTokensInfoAsync();
            if (string.IsNullOrEmpty(refreshToken) || tokensInfo.IsUpdated)
            {
                return tokensInfo.Tokens.Where(t => t.Name == "refresh_token").Select(t => t.Value).FirstOrDefault();
            }

            return refreshToken;
        }

        private async Task<string> GetTockenUpdatedTimeAsync()
        {
            // Include the access token in the properties
            var tockenUpdatedTime = await _context.GetTokenAsync("token_updated_time");
            var tokensInfo = await GetTokensInfoAsync();
            if (string.IsNullOrEmpty(tockenUpdatedTime) || tokensInfo.IsUpdated)
            {
                return tokensInfo.Tokens.Where(t => t.Name == "token_updated_time").Select(t => t.Value).FirstOrDefault();
            }

            return tockenUpdatedTime;
        }

        private async Task<IEnumerable<GdevApps.BLL.Models.AspNetUsers.AspNetUserToken>> GetAllUserTokens()
        {
            if (User.Identity.IsAuthenticated)
            {
                return await _aspUserService.GetAllTokensByUserIdAsync(_userManager.GetUserId(User));
            }

            return new List<GdevApps.BLL.Models.AspNetUsers.AspNetUserToken>();
        }

        private async Task<TokenResponse> GetTokensInfoAsync()
        {
            var allTokens = await GetAllUserTokens();
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

    internal sealed class Singleton
    {
        private static Singleton instance = null;
        private static readonly object padlock = new object();
        public readonly List<ClassSheetsViewModel> Sheets;
        public string ExternalAccessToken;
        Singleton()
        {
            Sheets = new List<ClassSheetsViewModel>();
            ExternalAccessToken = "";
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

    internal sealed class TokenResponse
    {
        public TokenResponse(bool isUpdated, IEnumerable<GdevApps.BLL.Models.AspNetUsers.AspNetUserToken> tokens)
        {
            this.IsUpdated = isUpdated;
            this.Tokens = tokens;
        }
        public bool IsUpdated { get; set; }

        public IEnumerable<GdevApps.BLL.Models.AspNetUsers.AspNetUserToken> Tokens { get; set; }
    }
}