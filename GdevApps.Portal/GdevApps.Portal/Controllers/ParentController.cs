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
using GdevApps.Portal.Models.TeacherViewModels;
using GdevApps.Portal.Services;
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

        public ParentController(UserManager<ApplicationUser> userManager,
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


        public IActionResult Index()
        {
            var userCurrentRole = HttpContext.Session.GetString("UserCurrentRole");  
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Authorize()]
        [VerifyUserRole(UserRoles.Parent)]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [HttpGet]
        public IActionResult GetStudents(){
            //TODO: create method to return a list of students
            var studentEmails = new List<string>(){
                "example@gmail.com",
                "example1@gmail.com"
            };

            var studentEmailsList = new SelectList(studentEmails, "Id", "Name");
            return View("ParentStudents", new ParentStudentsViewModel { Students = studentEmailsList });
        }

        [HttpPost]
        public async Task<IActionResult> GetClasses(string classId)
        {
            //TODO: Get Classes names
            //find gradebookId from parentstudent by parent email and student email
            //get the class name
            var classes = new List<object>(){
                new {
                    Id = 1,
                    Name = "test class"
                },
                new {
                    Id = 2,
                    Name = "test class second"
                },
            };

            return Json(classes);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}