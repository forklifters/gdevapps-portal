using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GdevApps.Portal.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using GdevApps.Portal.Models.AccountViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using GdevApps.BLL.Models.AspNetUsers;

namespace GdevApps.Portal.Attributes
{
    public class VerifyUserRole : ActionFilterAttribute
    {
        private readonly string _role;

        public VerifyUserRole(string role)
        {
            _role = role;
        }


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var userCurrentRole = filterContext.HttpContext.Session.GetString("UserCurrentRole");
            if (string.IsNullOrWhiteSpace(userCurrentRole))
            {
                filterContext.Result = new RedirectToRouteResult(
                                            new RouteValueDictionary{
                                                { "controller", "account" },
                                                { "action", "logoutFromAttr" }
                                                                     });
            }
            else if (userCurrentRole != _role && userCurrentRole != UserRoles.Admin)
            {
                switch (userCurrentRole)
                {
                    case UserRoles.Student:
                        break;
                    case UserRoles.Parent:
                        filterContext.Result = new RedirectToActionResult("", "Parent", "");
                        break;
                    case UserRoles.Teacher:
                        filterContext.Result = new RedirectToActionResult("Classes", "Teacher", "");
                        break;
                }
            }
        }
    }
}
