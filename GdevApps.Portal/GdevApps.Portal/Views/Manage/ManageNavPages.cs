using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GdevApps.Portal.Views.Manage
{
    public static class ManageNavPages
    {
        public static string ActivePageKey => "ActivePage";

        public static string Index => "Index";

        public static string ChangePassword => "ChangePassword";

        public static string ExternalLogins => "ExternalLogins";
        public static string Teachers => "Teachers";
        public static string Parents => "Parents";
        public static string Users => "Users";
        public static string AddUser => "AddUser";
        public static string AddRole => "AddRole";
        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);
        public static string TeachersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Teachers);
        public static string ParentsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Parents);
        public static string UsersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Users);
        public static string AddUserNavClass(ViewContext viewContext) => PageNavClass(viewContext, AddUser);
        public static string AddRoleNavClass(ViewContext viewContext) => PageNavClass(viewContext, AddUser);

        public static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string;
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }

        public static void AddActivePage(this ViewDataDictionary viewData, string activePage) => viewData[ActivePageKey] = activePage;
    }
}
