using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using GdevApps.BLL.Contracts;
using GdevApps.Portal.Attributes;
using GdevApps.Portal.Data;
using GdevApps.Portal.Models;
using GdevApps.Portal.Models.AccountViewModels;
using GdevApps.Portal.Models.SharedViewModels;
using GdevApps.Portal.Models.TeacherViewModels;
using GdevApps.Portal.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace GdevApps.Portal.Controllers
{
    [Authorize(Roles = UserRoles.Parent)]
    [VerifyUserRole(UserRoles.Parent)]
    public class ParentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly HttpContext _context;
        private readonly IAspNetUserService _aspUserService;
        private readonly IGdevSpreadsheetService _spreadSheetService;

        public ParentController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IMapper mapper,
            IHttpContextAccessor httpContext,
            IAspNetUserService aspUserService,
            IGdevSpreadsheetService spreadSheetService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _context = httpContext.HttpContext;
            _aspUserService = aspUserService;
            _spreadSheetService = spreadSheetService;
        }

         public IActionResult Index()
        {
            return RedirectToAction("GetStudents");
        }

          [HttpPost]
        public async Task<IActionResult> GetClasses(string studentEmail)
        {
            var user = await _userManager.GetUserAsync(User);
            var parentEmail = user.Email; 
            var mainGradebooks = await _spreadSheetService.GetMainGradebooksByParentEmailAndStudentEmailAsync(parentEmail, studentEmail);
            var classes = mainGradebooks.Select(g => new { 
                Id = g.GoogleUniqueId,
                Name = g.Name
            }).ToList();

            return Json(classes);
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents()
        {
            var user = await _userManager.GetUserAsync(User);
            var parentEmail = user.Email;
            var students = await _spreadSheetService.GetGradebookStudentsByParentEmailAsync(parentEmail);
            var studentEmails = students.Select(s => new StudentsViewModel
            {
                Id = s.Email,
                Name = s.Name ?? s.Email
            });

            var studentEmailsList = new SelectList(studentEmails, "Id", "Name");
            var studentsModel = new StudentReportsViewModel
            {
                Students = studentEmailsList,
                StudentReports = new List<ReportsViewModel>()
            };
            return View("ParentStudents", studentsModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetReport(string mainGradeBookId)
        {
            var userId = _userManager.GetUserId(User);
            var accessToken = await GetAccessTokenAsync();
            var refreshToken = await GetRefreshTokenAsync();
            var gradebookId = await _spreadSheetService.GetParentGradebookUniqueIdByMainGradebookIdAsync(mainGradeBookId);
            var settings = await _spreadSheetService.GetSettingsFromParentGradeBookAsync(accessToken, refreshToken, userId, gradebookId);
            var student = await _spreadSheetService.GetStudentInformationFromParentGradeBook(accessToken, refreshToken, userId, gradebookId);
             var user = await _userManager.GetUserAsync(User);
            var parentEmail = user.Email; 

            var report = _spreadSheetService.GetStudentReportInformation(
                accessToken,
                refreshToken,
                userId,
                student.ResultObject,
                settings.ResultObject,
                parentEmail);

            var students = await _spreadSheetService.GetGradebookStudentsByParentEmailAsync(parentEmail);
            var studentEmails = students.Select(s => new StudentsViewModel
            {
                Id = s.Email,
                Name = s.Name ?? s.Email
            });

            var studentEmailsList = new SelectList(studentEmails, "Id", "Name");

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

        #region Private
        
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

        #endregion
    }
}