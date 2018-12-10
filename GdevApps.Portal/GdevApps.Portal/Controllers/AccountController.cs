using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GdevApps.Portal.Data;
using GdevApps.Portal.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using GdevApps.Portal.Services;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using GdevApps.BLL.Models.AspNetUsers;
using GdevApps.BLL.Contracts;
using Microsoft.AspNetCore.Http;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using Serilog;

namespace GdevApps.Portal.Controllers
{
    [Route("[controller]/[action]")]
    [Authorize(Roles = "Admin, Teacher, Parent")]

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IAspNetUserService _aspNetUserService;

        //TODO: Create own list of animal faces
        private const string _defaultAvatar = "https://www.dropbox.com/s/5r1f49l2zx5e2yv/quokka_lg.png?raw=1";

        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger logger,
            IConfiguration configuration,
            IAspNetUserService aspNetUserService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
            _aspNetUserService = aspNetUserService;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            _logger.Debug("Login page with returnUrl: {ReturnUrl} was requested. Clear the existing external cookie to ensure a clean login process", returnUrl);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAdmin(string returnUrl = null)
        {
            _logger.Debug("Login as admin page with returnUrl: {ReturnUrl} was requested. Clear the existing external cookie to ensure a clean login process", returnUrl);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //  [HttpGet]
        // [AllowAnonymous]
        // public async Task<IActionResult> ConfirmEmail(string userId, string code)
        // {
        //     if (userId == null || code == null)
        //     {
        //         return RedirectToAction(nameof(HomeController.Index), "Home");
        //     }
        //     var user = await _userManager.FindByIdAsync(userId);
        //     if (user == null)
        //     {
        //         throw new ApplicationException($"Unable to load user with ID '{userId}'.");
        //     }
        //     var result = await _userManager.ConfirmEmailAsync(user, code);
        //     return View(result.Succeeded ? "ConfirmEmail" : "Error");
        // }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAdmin(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    HttpContext.Session.SetString("UserCurrentRole", UserRoles.Admin);
                    _logger.Information("User: {Email} successfully logged in as Admin.", model.Email);
                    return RedirectToLocal(returnUrl);
                }
                // if (result.RequiresTwoFactor)
                // {
                //     return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                // }
                if (result.IsLockedOut)
                {
                    _logger.Warning("User account: {Email} is locked out.", model.Email);
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    _logger.Warning("Invalid login attempt for user account: {Email}.", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            _logger.Debug("User: {Email} attempted to log in as Admin. Login Model was incorrect.", model?.Email);
            return View(model);
        }

        // [HttpPost]
        // [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        // public IActionResult Login(LoginViewModel model, string returnUrl = null)
        // {
        //     ViewData["ReturnUrl"] = returnUrl;
        //     if (ModelState.IsValid)
        //     {
        //         //do stuff
        //     }

        //     // If we got this far, something failed, redisplay form
        //     return View(model);
        // }

        // [HttpGet]
        // [AllowAnonymous]
        // public IActionResult Register(string returnUrl = null)
        // {
        //     ViewData["ReturnUrl"] = returnUrl;
        //     return View();
        // }

        // [HttpPost]
        // [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        // {
        //     ViewData["ReturnUrl"] = returnUrl;
        //     if (ModelState.IsValid)
        //     {
        //         var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
        //         var result = await _userManager.CreateAsync(user, model.Password);
        //         if (result.Succeeded)
        //         {
        //             _logger.Information("User created a new account with password.");
        //             // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
        //             // Send an email with this link
        //             //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //             //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
        //             //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

        //             await _signInManager.SignInAsync(user, isPersistent: false);
        //             _logger.Information("User created a new account with password.");
        //             return RedirectToLocal(returnUrl);
        //         }
        //         AddErrors(result);
        //     }

        //     // If we got this far, something failed, redisplay form
        //     return View(model);
        // }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            await _signInManager.SignOutAsync();
            _logger.Information("User: {Email} has successfully logged out. Clearing session and redirecting to the home page", user?.Email);
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LogoutFromAttr()
        {
            var user = await _userManager.GetUserAsync(User);
            await _signInManager.SignOutAsync();
            _logger.Information("User: {Email} has successfully logged out. Clearing session and redirecting to the home page", user?.Email);
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            _logger.Debug("ExternalLogin was executed for provider: {Provider} with returnUrl: {Url}", provider, returnUrl);
            if (ModelState.IsValid)
            {
                var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
                _logger.Information("Requesting a redirect to the external login provider: {Provider}. Redirect url: {Url}", provider, redirectUrl);
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

                return Challenge(properties, provider);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RequestAccount(RequestAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                _logger.Debug("Request account page was requested for {Name} with email {Email}. Reqson: {Message}.", model?.Name, model?.Email, model?.Message);
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Avatar = model.Avatar
                };
                //TODO: EMAIL WITH A REQUEST SHOULD BE SENT TO GDEVAPPS ACCOUNT
                // var result = await _userManager.CreateAsync(user);
                //REMOVE THIS CODE. 
                // if (result.Succeeded)
                // {
                //     _logger.Information("User: {Email} successfully created a new account with password.", model.Email);
                //     // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                //     // Send an email with this link
                //     //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //     //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                //     //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                //     await _signInManager.SignInAsync(user, isPersistent: false);
                //     _logger.Information("User created a new account with password.");
                //     return RedirectToLocal("/");
                // }

                // if (result.Errors.Count() == 0)
                // {
                //     ViewBag.result = @"<script type='text/javascript'>
                //                 $( document ).ready(function() {
                //                 BootstrapDialog.show({
                //                             type: BootstrapDialog.TYPE_SUCCESS,
                //                             title: 'Request an account',
                //                             message: 'Request Submitted Successfully!',
                //                         });
                //                 });
                //             </script>";
                // }else{
                //     ViewBag.result = @"<script type='text/javascript'>
                //                 $( document ).ready(function() {
                //                 BootstrapDialog.show({
                //                             type: BootstrapDialog.TYPE_DANGER,
                //                             title: 'Request an account',
                //                             message: 'An error occurred during the request submission!',
                //                         });
                //                 });
                //             </script>";
                // }

                return View();
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            _logger.Debug("External login callback was executed with returnUrl: {ReturnUrl}", returnUrl);
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}, returnUrl: {returnUrl}";
                _logger.Error(ErrorMessage);
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.Warning("External login info is null. Redirecting to Login page.");
                return RedirectToAction(nameof(Login));
            }

            // Get user profile picture 
            var picture = info.Principal.FindFirstValue("image") ?? _defaultAvatar;
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            // Sign in the user with this external login provider if the user already has a login.
            _logger.Information("Trying to sign in user with provider: {Provider} and provider key: {ProviderKey}", info.LoginProvider, info.ProviderKey);
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            //save access_token token_type, expires_at
            if (result.Succeeded)
            {
                _logger.Information("User was successfully signed in. Saving tokens: access_token, token_type, expires_at. Including the access token in the properties");
                var user = await this._userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user == null)
                {
                    _logger.Warning("User was not found by login provider and key combination. Trying to find by email {Email}", email);
                    user = await _userManager.FindByEmailAsync(email);
                }
                await SignInAndUpdeteTokens(info, user);
                _logger.Debug("Updating user {Email} avatar", user.Email);
                user.Avatar = picture;
                await _userManager.UpdateAsync(user);

                return RedirectToAction(nameof(MultipleSignIn), new { user.Email, returnUrl });
            }
            else
            {
                _logger.Information("User {Email} was not signed in using provider: {Provider}", email, info.LoginProvider);
                var teacher = await _aspNetUserService.GetTeacherByEmailAsync(email);
                var parent = await _aspNetUserService.GetParentByEmailAsync(email);
                if (teacher != null)
                {
                    _logger.Information("User {Email} exists as teacher with id {TeacherId}. Creating a new account.", email, teacher.Id);
                    var applicationUser = new ApplicationUser
                    {
                        UserName = teacher.Name,
                        Email = email,
                        Avatar = teacher.Avatar ?? picture
                    };

                    var userCreationResult = await _userManager.CreateAsync(applicationUser);
                    var createLogin = await _userManager.AddLoginAsync(applicationUser, info);

                    if (userCreationResult.Succeeded && createLogin.Succeeded)
                    {
                        _logger.Information("User {Email} created a new account with password successfully. Saving tokens: access_token, token_type, expires_at. Including the access token in the properties", email);
                        var user = await this._userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                        if (user == null)
                        {
                            _logger.Warning("User was not found by login provider and key combination. Trying to find by email {Email}", email);
                            user = await _userManager.FindByEmailAsync(email);
                        }

                        _logger.Information("Connect user {Email} to the asp user with id {AspUserId}", email, user.Id);
                        await _aspNetUserService.SetTeacherAspUserIdAsync(teacher.Id, user.Id);
                        _logger.Information("Add user {Email} to the role {Role}", email, UserRoles.Teacher);
                        await _userManager.AddToRoleAsync(user, UserRoles.Teacher);
                        await SignInAndUpdeteTokens(info, user);

                        return RedirectToAction(nameof(MultipleSignIn), new { user.Email, returnUrl });
                    }
                }
                else if (parent != null)
                {
                    _logger.Information("User {Email} exists as parent with id {ParentId}. Creating a new account.", email, parent.Id);
                    var applicationUser = new ApplicationUser
                    {
                        UserName = parent.Name,
                        Email = email,
                        Avatar = parent.Avatar
                    };

                    var userCreationResult = await _userManager.CreateAsync(applicationUser);
                    var createLogin = await _userManager.AddLoginAsync(applicationUser, info);

                    if (userCreationResult.Succeeded && createLogin.Succeeded)
                    {
                        _logger.Information("User {Email} created a new account with password successfully. Saving tokens: access_token, token_type, expires_at. Including the access token in the properties", email);
                        var user = await this._userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                        if (user == null)
                        {
                            _logger.Warning("User was not found by login provider and key combination. Trying to find by email {Email}", email);
                            user = await _userManager.FindByEmailAsync(email);
                        }

                        _logger.Information("Connect user {Email} to the asp user with id {AspUserId}", email, user.Id);
                        await _aspNetUserService.SetParentAspUserIdAsync(parent.Id, user.Id);
                        _logger.Information("Add user {Email} to the role {Role}", email, UserRoles.Parent);
                        await _userManager.AddToRoleAsync(user, UserRoles.Parent);
                        await SignInAndUpdeteTokens(info, user);

                        return RedirectToAction(nameof(MultipleSignIn), new { user.Email, returnUrl });
                    }
                }
            }
            if (result.IsLockedOut)
            {
                _logger.Warning("User {Email} is locked out. Redirecting to lockout page.", email);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.Information("User {Email} does not have an account. Redirect user to the request account page.", email);
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;

                return View("RequestAccount", new RequestAccountViewModel
                {
                    Email = email,
                    Avatar = picture
                });
            }
        }

        //TODO: Test function
        private async Task SignInAndUpdeteTokens(ExternalLoginInfo info, ApplicationUser user)
        {
            var props = new AuthenticationProperties();
            props.IsPersistent = true;
            props.ExpiresUtc = DateTime.UtcNow.AddHours(1);
            props.StoreTokens(info.AuthenticationTokens);
            await _signInManager.SignInAsync(user, props, info.LoginProvider);
            _logger.Debug("Updating external authentication token for user {Email}", user.Email);
            await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
            _logger.Information("User {Email} logged in with provider {Name}.", user.Email, info.LoginProvider);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Teacher)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInAsTeacher(string email)
        {
            var user = await _userManager.GetUserAsync(User);
            _logger.Information("User {Email} decided to proceed as a {Role}}", user.Email, UserRoles.Teacher);
            HttpContext.Session.SetString("UserCurrentRole", UserRoles.Teacher);
            return RedirectToAction(nameof(TeacherController.Classes), "Teacher");
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Parent)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInAsParent(string email)
        {
            var user = await _userManager.GetUserAsync(User);
            _logger.Information("User {Email} decided to proceed as a {Role}}", user.Email, UserRoles.Teacher);
            HttpContext.Session.SetString("UserCurrentRole", UserRoles.Parent);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MultipleSignIn(string email, string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            //do stuff
            //check if user is in USers table (teacher)
            var parent = await _aspNetUserService.GetParentByEmailAsync(email);
            var teacher = await _aspNetUserService.GetTeacherByEmailAsync(email);
            var loginInfo = new AccountLoginInfo
            {
                isParent = parent != null,
                isTeacher = teacher != null
            };

            return View("MultipleSignInModel", loginInfo);
        }

        // [HttpPost]
        // [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         // Get the information about the user from the external login provider
        //         var info = await _signInManager.GetExternalLoginInfoAsync();
        //         if (info == null)
        //         {
        //             _logger.LogError("Error loading external login information during confirmation. External login info is empty for user {Email}", model.Email);
        //             throw new ApplicationException("Error loading external login information during confirmation.");
        //         }
        //         var user = new ApplicationUser
        //         {
        //             UserName = model.Email,
        //             Email = model.Email
        //         };
        //         var result = await _userManager.CreateAsync(user);
        //         if (result.Succeeded)
        //         {
        //             result = await _userManager.AddLoginAsync(user, info);
        //             if (result.Succeeded)
        //             {
        //                 await _signInManager.SignInAsync(user, isPersistent: false);
        //                 _logger.Information("User created an account using {Name} provider.", info.LoginProvider);
        //                 return RedirectToLocal(returnUrl);
        //             }
        //         }
        //         AddErrors(result);
        //     }

        //     ViewData["ReturnUrl"] = returnUrl;
        //     return View(nameof(ExternalLogin), model);
        // }

        // [HttpGet]
        // [AllowAnonymous]
        // public async Task<IActionResult> ConfirmEmail(string userId, string code)
        // {
        //     if (userId == null || code == null)
        //     {
        //         return RedirectToAction(nameof(HomeController.Index), "Home");
        //     }
        //     var user = await _userManager.FindByIdAsync(userId);
        //     if (user == null)
        //     {
        //         throw new ApplicationException($"Unable to load user with ID '{userId}'.");
        //     }
        //     var result = await _userManager.ConfirmEmailAsync(user, code);
        //     return View(result.Succeeded ? "ConfirmEmail" : "Error");
        // }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }


//TODO: Add method to restore password
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            // if (ModelState.IsValid)
            // {
            //     var user = await _userManager.FindByEmailAsync(model.Email);
            //     if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            //     {
            //         // Don't reveal that the user does not exist or is not confirmed
            //         return RedirectToAction(nameof(ForgotPasswordConfirmation));
            //     }

            //     // For more information on how to enable account confirmation and password reset please
            //     // visit https://go.microsoft.com/fwlink/?LinkID=532713
            //     var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            //     var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
            //     await _emailSender.SendEmailAsync(model.Email, "Reset Password",
            //        $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
            //     return RedirectToAction(nameof(ForgotPasswordConfirmation));
            // }

            // // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //TEST METHOD
        //         [HttpGet]
        //         [AllowAnonymous]
        //         public async Task<IActionResult> SetMyDefaultPassword()
        //         {
        //             var user = await _userManager.FindByEmailAsync("karac38@gmail.com");
        //             await _userManager.AddPasswordAsync(user, "123321zim");    
        //             return Ok();
        //         }

        //TEST METHOD
        //          [HttpGet]
        //         [AllowAnonymous]
        //         public async Task<IActionResult> SetMyDefaultRole()
        //         {
        //             var user = await _userManager.FindByEmailAsync("karac38@gmail.com");
        //             await _userManager.AddToRoleAsync(user, "Admin");    
        //             return Ok();
        //         }

        //TODO: Create a method to reset the password

        //  [HttpGet]
        // [AllowAnonymous]
        // public IActionResult ResetPassword(string code = null)
        // {
        //     if (code == null)
        //     {
        //         _logger.LogError("User tried to reser the pasword without the code. A code must be supplied for password reset");
        //         throw new ApplicationException("A code must be supplied for password reset.");
        //     }
        //     var model = new ResetPasswordViewModel { Code = code };
        //     return View(model);
        // }
        // [HttpPost]
        // [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return View(model);
        //     }
        //     var user = await _userManager.FindByEmailAsync(model.Email);
        //     if (user == null)
        //     {
        //         // Don't reveal that the user does not exist
        //         return RedirectToAction(nameof(ResetPasswordConfirmation));
        //     }
        //     var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        //     if (result.Succeeded)
        //     {
        //         return RedirectToAction(nameof(ResetPasswordConfirmation));
        //     }
        //     AddErrors(result);
        //     return View();
        // }

        // [HttpGet]
        // [AllowAnonymous]
        // public IActionResult ResetPasswordConfirmation()
        // {
        //     return View();
        // }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddTeacher()
        {
            return PartialView("_AddTeacher", new UserViewModel() { Role = UserRoles.Teacher });
        }

        [HttpPost]
        public async Task<ActionResult> AddTeacher(UserViewModel userModel)
        {
            if (ModelState.IsValid)
            {
                _logger.Information("Adding a new teacher {Email} with name {Name}", userModel.Email, userModel.Name);
                var dbUser = await _userManager.FindByEmailAsync(userModel.Email);
                if (dbUser != null)
                {
                    _logger.Warning("User {Email} already exists.", userModel.Email);
                    ModelState.AddModelError("Email", $"User with email {userModel.Email} already exists");
                    return PartialView("_AddTeacher", userModel);
                }

                var userId = _userManager.GetUserId(User);
                var teacherModel = new BLL.Models.AspNetUsers.Teacher()
                {
                    Avatar = _defaultAvatar,
                    Email = userModel.Email,
                    Name = userModel.Name,
                    CreatedBy = userId
                };

                var result = await _aspNetUserService.AddTeacherAsync(teacherModel);
                if (result)
                {
                    _logger.Information("User {Email} was successfully added as a teacher", userModel.Email);
                    return Ok();
                }
                else
                {
                    _logger.Error("User {Email} was not added as a teacher", userModel.Email);
                    return BadRequest(userModel.Email);
                }
            }

            return PartialView("_AddTeacher", userModel);
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
