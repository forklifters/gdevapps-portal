using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GdevApps.Portal.Data;
using GdevApps.Portal.Models;
using GdevApps.Portal.Models.AccountViewModels;
using GdevApps.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GdevApps.Portal.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();

            var userCurrentRole = HttpContext.Session.GetString("UserCurrentRole");
            if (!string.IsNullOrWhiteSpace(userCurrentRole))
            {
                switch (userCurrentRole)
                {
                    case UserRoles.Student:
                        break;
                    case UserRoles.Parent:
                        return RedirectToAction("Index", "Parent");
                    case UserRoles.Teacher:
                        return RedirectToAction("Index", "Teacher");
                    default:
                        return RedirectToAction("logoutFromAttr", "Account");
                }
            }

            return RedirectToAction("LogoutFromAttr", "Account");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}