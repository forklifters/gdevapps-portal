using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

namespace GdevApps.Portal.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IAspNetUserService _aspNetUserService;

        private const string _defaultAvatar = "https://www.dropbox.com/s/5r1f49l2zx5e2yv/quokka_lg.png?raw=1";

        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
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
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

         [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAdmin(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

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
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                //do stuff
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created a new account with password.");
                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            //clear session
            HttpContext.Session.Clear();  
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LogoutFromAttr()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            //clear session
            HttpContext.Session.Clear();  
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Request a redirect to the external login provider.
                var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
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
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, Avatar = model.Avatar };
                var result = await _userManager.CreateAsync(user);

                //REMOVE THIS CODE
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                    // Send an email with this link
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created a new account with password.");
                    return RedirectToLocal("/");
                }

                if (result.Errors.Count() == 0)
                {
                    ViewBag.result = @"<script type='text/javascript'>
                                $( document ).ready(function() {
                                BootstrapDialog.show({
                                            type: BootstrapDialog.TYPE_SUCCESS,
                                            title: 'Request an account',
                                            message: 'Request Submitted Successfully!',
                                        });
                                });
                            </script>";
                }else{
                    ViewBag.result = @"<script type='text/javascript'>
                                $( document ).ready(function() {
                                BootstrapDialog.show({
                                            type: BootstrapDialog.TYPE_DANGER,
                                            title: 'Request an account',
                                            message: 'An error occurred during the request submission!',
                                        });
                                });
                            </script>";
                }

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
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Get user profile picture 
            var picture = info.Principal.FindFirstValue("image");
            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            //save access_token token_type, expires_at
            if (result.Succeeded)
            {
                //Include the access token in the properties
                var user =  await this._userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var props = new AuthenticationProperties();
                props.IsPersistent = true;
                props.ExpiresUtc = DateTime.UtcNow.AddDays(5);
                props.StoreTokens(info.AuthenticationTokens);
                await _signInManager.SignInAsync(user, props, info.LoginProvider);

                // Add this to add token to datastore
                // Update the token
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                //return RedirectToLocal(returnUrl);
                return RedirectToAction(nameof(MultipleSignIn), new { user.Email, returnUrl });
            }else{
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var teacher = await _aspNetUserService.GetTeacherByEmailAsync(email);
                var parent = await _aspNetUserService.GetParentByEmailAsync(email);
                if (teacher != null)
                {
                    var applicationUser = new ApplicationUser
                    {
                        UserName = teacher.Name,
                        Email = email,
                        Avatar = teacher.Avatar
                    };
                    var userCreationResult = await _userManager.CreateAsync(applicationUser);
                    var createLogin = await _userManager.AddLoginAsync(applicationUser, info);

                    if (userCreationResult.Succeeded && createLogin.Succeeded)
                    {
                        //TODO: Update teacher same as parent asp user id
                        _logger.LogInformation("User created a new account with password.");

                        //Include the access token in the properties
                        var user = await this._userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                        var props = new AuthenticationProperties();
                        props.IsPersistent = true;
                        props.ExpiresUtc = DateTime.UtcNow.AddDays(5);
                        props.StoreTokens(info.AuthenticationTokens);
                        await _signInManager.SignInAsync(user, props, info.LoginProvider);

                        // Add this to add token to datastore
                        // Update the token
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                        _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                        //return RedirectToLocal(returnUrl);
                        return RedirectToAction(nameof(MultipleSignIn), new { user.Email, returnUrl });

                        _logger.LogInformation("User created a new account with password.");
                        return RedirectToLocal("/");
                    }
                }
                else if (parent != null)
                {
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
                        _logger.LogInformation("User created a new account with password.");
                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                        //Include the access token in the properties
                        var user = await this._userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                        if (user == null)
                        {
                            user = await _userManager.FindByEmailAsync(email);
                        }

                        //Update parent
                        await _aspNetUserService.SetParentAspUserId(parent.Id, user.Id);

                        var props = new AuthenticationProperties();
                        props.IsPersistent = true;
                        props.ExpiresUtc = DateTime.UtcNow.AddDays(5);
                        props.StoreTokens(info.AuthenticationTokens);
                        await _signInManager.SignInAsync(user, props, info.LoginProvider);

                        // Add this to add token to datastore
                        // Update the token
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                        _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                        //return RedirectToLocal(returnUrl);
                        return RedirectToAction(nameof(MultipleSignIn), new { user.Email, returnUrl });


                        return RedirectToLocal("/");
                    }
                }
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                return View("RequestAccount", new RequestAccountViewModel
                {
                    Email = email,
                    Avatar = string.IsNullOrEmpty(picture) ? _defaultAvatar : picture
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInAsTeacher(string email)
        {
            HttpContext.Session.SetString("UserCurrentRole", UserRoles.Teacher);  
            return RedirectToAction(nameof(TeacherController.ClassesAsync), "Teacher");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInAsParent(string email)
        {
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
            //TODO: Get teacher
            var loginInfo = new AccountLoginInfo
            {
                isParent = parent != null,
                isTeacher = true
            };

            return View("MultipleSignInModel", loginInfo);
        }




        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Id, code, Request.Scheme);
                await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                   $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddTeacher()
        {
            return PartialView("_AddTeacher", new UserViewModel() { Role = UserRoles.Parent });
        }

        [HttpPost]
        public async Task<ActionResult> AddTeacher(UserViewModel userModel)
        {
            if (ModelState.IsValid)
            {
                var dbUser = await _userManager.FindByEmailAsync(userModel.Email);
                if(dbUser != null){
                     ModelState.AddModelError("Email", $"User with email {userModel.Email} already exists");
                    return PartialView("_AddTeacher", userModel);
                }

                var parentModel = new Parent()
                {
                    Avatar = _defaultAvatar,
                    Email = userModel.Email,
                    Name = userModel.Name
                };

                var result = _aspNetUserService.AddParent(parentModel);
                if (result)
                {
                    return Ok();
                }
                else
                {
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
