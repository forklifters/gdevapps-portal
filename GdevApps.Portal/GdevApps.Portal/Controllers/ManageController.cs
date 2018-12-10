using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AutoMapper;
using GdevApps.BLL.Contracts;
using GdevApps.BLL.Models.AspNetUsers;
using GdevApps.DAL.DataModels.AspNetUsers.AspNetUser;
using GdevApps.Portal.Data;
using GdevApps.Portal.Models.ManageViewModels;
using GdevApps.Portal.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Serilog;

namespace GdevApps.Portal.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [Authorize(Roles = "Admin")]

    public class ManageController : Controller
    {
         private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly UrlEncoder _urlEncoder;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAspNetUserService _aspUserService;
        private readonly IMapper _mapper;
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        private const string RecoveryCodesKey = nameof(RecoveryCodesKey);
        private const string _defaultAvatar = "https://www.dropbox.com/s/5r1f49l2zx5e2yv/quokka_lg.png?raw=1";

        public ManageController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IEmailSender emailSender,
          ILogger logger,
          UrlEncoder urlEncoder,
          RoleManager<IdentityRole> roleManager,
          IAspNetUserService aspNetUserService,
          IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _urlEncoder = urlEncoder;
            _roleManager = roleManager;
            _aspUserService = aspNetUserService;
            _mapper = mapper;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                var userId = _userManager.GetUserId(User);
                _logger.Error("Unable to load information for user with id {UserId}", userId);
                throw new ApplicationException($"Unable to load user with Id '{userId}'.");
            }

            var roles = _roleManager.Roles.ToList();
            var model = new IndexViewModel
            {
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = user.EmailConfirmed,
                StatusMessage = StatusMessage
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Teachers()
        {
            _logger.Debug("User {UserId} requested Teachers page", _userManager.GetUserId(User));
            var users = await _aspUserService.GetAllTeachersAsync();
            _logger.Debug("{Number} teachers were found", users.Count);
            var portalUsers = _mapper.Map<IEnumerable<GdevApps.Portal.Models.ManageViewModels.PortalUserViewModel>>(users);
            
            return View(portalUsers);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTeachers(PortalUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                _logger.Information("User {UserId} is trying to delete teacher {TeacherEmail} with teacher id: {TeacherId}", _userManager.GetUserId(User), model.Email, model.Id);
                var result = await _aspUserService.DeleteTeacherByIdAsync(model.Role.RoleId);
                if (!result)
                {
                    _logger.Error("Teacher {Email} with id {TeacherId} was not deleted", model.Email, model.Id);
                    return BadRequest("Teacher was not deleted");
                }
                var userToUnsign = await _userManager.FindByEmailAsync(model.Email);
                if (userToUnsign != null)
                {
                    var unsignResult = await _userManager.RemoveFromRoleAsync(userToUnsign, UserRoles.Teacher);
                    if (!unsignResult.Succeeded)
                    {
                        _logger.Error("An error occurred during removing user {UserEmail} from role {Role}. Error: {Error}", model.Email, UserRoles.Teacher, unsignResult.ToString());
                        return BadRequest($"User was not removed from the {UserRoles.Teacher} role");
                    }
                }

                _logger.Information("User {UserEmail} was successfully removed from role {Role}", model.Email, UserRoles.Teacher);
                return RedirectToAction("Teachers");
            }

            _logger.Warning("Teacher model is not valid");
            return BadRequest("Teacher model is not valid");
        }

        [HttpGet]
        public IActionResult AddTeacher()
        {
            _logger.Debug("AddTeacher page was requested");
            return PartialView("_AddTeacher", new AddRoleViewModel(UserRoles.Teacher));
        }

        [HttpPost]
        public async Task<IActionResult> AddTeacher(AddRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                _logger.Information("User with id {UserId} is trying to add a Teacher {Email}", userId, model.Email);
                var teacherExists = (await _aspUserService.GetTeacherByEmailAsync(model.Email)) != null;
                if (teacherExists)
                {
                    _logger.Warning("Teacher with email {Email} already exists", model.Email);
                    ModelState.AddModelError("Email", $"Current user already registered as a teacher");
                    return PartialView("_AddTeacher", model);
                }

                var teacher = new BLL.Models.AspNetUsers.Teacher()
                {
                    Avatar = _defaultAvatar,
                    Email = model.Email,
                    Name = model.Name,
                    CreatedBy = userId
                };

                var result = await _aspUserService.AddTeacherAsync(teacher);
                if (result)
                {
                    _logger.Information("Teacher {email} was successfully added", model.Email);
                    return Ok();
                }
                else
                {
                    _logger.Error("Teacher {email} was not added", model.Email);
                    return BadRequest("Teacher was not added");
                }
            }

            return PartialView("_AddTeacher", model);
        }

        [HttpGet]
        public async Task<IActionResult> Parents()
        {
            _logger.Debug("User {UserId} requested Parents page", _userManager.GetUserId(User));
            var users = await _aspUserService.GetAllParentsAsync();
            _logger.Debug("{Number} parents were found", users.Count);
            var parents = _mapper.Map<IEnumerable<GdevApps.Portal.Models.ManageViewModels.PortalUserViewModel>>(users);
            return View(parents);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveParent(PortalUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                _logger.Information("User {UserId} is trying to delete parent {ParentEmail} with teacher id: {ParentId}", _userManager.GetUserId(User), model.Email, model.Id);
                var result = await _aspUserService.DeleteParentByIdAsync(model.Role.RoleId);
                if (!result)
                {
                    _logger.Error("Parent {Email} with id {ParentId} was not deleted", model.Email, model.Id);
                    return BadRequest("Parent was not deleted");
                }
                var userToUnsign = await _userManager.FindByEmailAsync(model.Email);
                if (userToUnsign != null)
                {
                    var unsignResult = await _userManager.RemoveFromRoleAsync(userToUnsign, UserRoles.Teacher);
                    if (!unsignResult.Succeeded)
                    {
                        _logger.Error("An error occurred during removing user {UserEmail} from role {Role}. Error: {Error}", model.Email, UserRoles.Teacher, unsignResult.ToString());
                        return BadRequest($"User was not removed from the {UserRoles.Teacher} role");
                    }
                }

                _logger.Information("User {UserEmail} was successfully removed from the role {Role}", model.Email, UserRoles.Teacher);
                return RedirectToAction("Parents");
            }

            return BadRequest("Parent model is not valid");
        }


        [HttpGet]
        public async Task<IActionResult> Users()
        {
            _logger.Debug("User {UserId} requested Users page", _userManager.GetUserId(User));
            var users = await _aspUserService.GetAllUsersAsync();
            _logger.Debug("{Number} users were found", users.Count);
            var portalUsers = _mapper.Map<List<GeneralPortalUserViewModel>>(users);
            return View(portalUsers);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUser(GeneralPortalUserViewModel model)
        {
            _logger.Information("User {UserId} is trying to delete user {UserToDeleteEmail} with id {UserToDeleteId}", _userManager.GetUserId(User), model.Email, model.Id);
            var userToDelete = await _userManager.FindByIdAsync(model.Id);
            if (userToDelete == null)
            {
                _logger.Error("User {UserToDeleteEmail} with id {UserToDeleteId} was not found", model.Email, model.Id);
                return BadRequest($"User {model.Email} was not found");
            }

            var deleteResult = await _userManager.DeleteAsync(userToDelete);
            if(!deleteResult.Succeeded)
            {
                _logger.Error("User {UserToDeleteEmail} with id {UserToDeleteId} was not deleted. Error: {Error}", model.Email, model.Id, deleteResult.ToString());
            }
            return RedirectToAction("Users");
        }

        [HttpGet]
        public IActionResult AddUser()
        {
           return View(new AddUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = _userManager.GetUserId(User);
                _logger.Debug("User {UserId} is trying to add a new user {AddUserEmail} with a role {Role}", currentUserId, model.UserEmail, model.UserRole);
                var userExists = (await _userManager.FindByEmailAsync(model.UserEmail)) != null;
                if (userExists)
                {
                    _logger.Error("User {AddUserEmail} already registered as a {Role}", model.UserEmail, model.UserRole);
                    ModelState.AddModelError("Email", $"Current user already registered as a teacher");
                    return View("AddUser", model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.UserEmail,
                    Avatar = _defaultAvatar,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.Information("A new user {AddUserEmail} with role {Role} was created.", model.UserEmail, model.UserRole);
                }else
                {
                    _logger.Error("A new user {AddUserEmail} with role {Role} was not created.", model.UserEmail, model.UserRole);
                    return BadRequest();
                }

                _logger.Information("Trying to assign user {AddUserEmail} to the role {Role}", model.UserEmail, model.UserRole);
                switch (model.UserRole)
                {
                    case UserRoles.Admin:
                        await _userManager.AddToRoleAsync(user, model.UserRole);
                        break;
                    case UserRoles.Teacher:
                        var currentUser = await _userManager.GetUserAsync(User);
                        var teacher = new BLL.Models.AspNetUsers.Teacher()
                        {
                            AspNetUserId = user.Id,
                            Avatar = user.Avatar ?? _defaultAvatar,
                            CreatedBy = currentUser.Id,
                            CreatedByEmail = currentUser.Email,
                            Email = user.Email,
                            Name = user.UserName
                        };
                        var teacherAddResult = await _aspUserService.AddTeacherAsync(teacher);
                        if(!teacherAddResult)
                        {
                            _logger.Error("An error occured during the creating a {Role} role to the user {AddUserEmail}.", model.UserRole, model.UserEmail);
                            return BadRequest($"An error occured during the assigning a {model.UserRole} role");
                        }
                        var addToRoleResult = await _userManager.AddToRoleAsync(user, model.UserRole);
                        if(!addToRoleResult.Succeeded)
                        {
                            _logger.Error("An error occured during the assigning a {Role} role to the user {AddUserEmail}. Error: {Error}", model.UserRole, model.UserEmail, addToRoleResult.ToString());
                            return BadRequest($"An error occured during the assigning a {model.UserRole} role to the user.");
                        }
                        break;
                }

                return RedirectToAction("Users");
            }

            return View("AddUser", model);
        }

        [HttpGet]
        public async Task<IActionResult> AddUserRole(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.Error("User: {UserEmail} was not found", userEmail);
                return BadRequest("User was not found");
            } 
            var allowedUserRoles = new List<object>() {
                new{
                    id = "",
                    value = ""
                },
                new{
                    id = UserRoles.Admin,
                    value = UserRoles.Admin
                },
                 new{
                    id = UserRoles.Teacher,
                    value = UserRoles.Teacher
                },
                 };

            var roleViewModel = new GeneralAddRoleViewModel()
            {
                Roles = new SelectList(allowedUserRoles, "id", "value"),
                Email = user.Email,
                Name = user.UserName
            };

            return View("AddRole", roleViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserRole(GeneralAddRoleViewModel roleModel)
        {
            if (ModelState.IsValid)
            {
                _logger.Information("Add user role action was executed for user: {UserEmail}, user name: {UserName} role: {RoleName}", roleModel.Email, roleModel.Name, roleModel.RoleName);
                var user = await _userManager.FindByEmailAsync(roleModel.Email);
                if (user == null)
                {
                    _logger.Error("User: {UserEmail} was not found", roleModel.Email);
                    return BadRequest("User was not found");
                }
                if (await _userManager.IsInRoleAsync(user, roleModel.RoleName))
                {
                    _logger.Information("User: {UserEmail} is already in role: {Role}", roleModel.Email, roleModel.RoleName);
                    return BadRequest($"{roleModel.Name} is already in role {roleModel.RoleName}");
                }

                var currentUser = await _userManager.GetUserAsync(User);
                switch (roleModel.RoleName)
                {
                    case UserRoles.Teacher:
                        var teacher = new BLL.Models.AspNetUsers.Teacher()
                        {
                            AspNetUserId = user.Id,
                            Avatar = user.Avatar ?? _defaultAvatar,
                            CreatedBy = currentUser.Id,
                            CreatedByEmail = currentUser.Email,
                            Email = user.Email,
                            Name = user.UserName
                        };

                        var addTeacherResult = await _aspUserService.AddTeacherAsync(teacher);
                        if(!addTeacherResult)
                        {
                            _logger.Error("An error occured while creating a {Role} with email {UserEmail}", roleModel.RoleName, user.Email);
                            return BadRequest($"{roleModel.RoleName} was not assigned to the {user.Email}");
                        }
                        break;
                }

                var assignToRoleResult = await _userManager.AddToRoleAsync(user, roleModel.RoleName);
                if (!assignToRoleResult.Succeeded)
                {
                    _logger.Error("An error occured while assigning user {UserEmail} to the role {Role}", user.Email, roleModel.RoleName);
                    return BadRequest($"{roleModel.RoleName} was not assigned to the {user.Email}");
                }

                return RedirectToAction("Users");
            }

            //add roles again
            var allowedUserRoles = new List<object>() {
                new{
                    id = "",
                    value = ""
                },
                new{
                    id = UserRoles.Admin,
                    value = UserRoles.Admin
                },
                 new{
                    id = UserRoles.Teacher,
                    value = UserRoles.Teacher
                },
            };
            roleModel.Roles = new SelectList(allowedUserRoles, "id", "value");
            return View("AddRole", roleModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = user.Email;
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            var phoneNumber = user.PhoneNumber;
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }
            }

            StatusMessage = "Your profile has been updated";
            return RedirectToAction(nameof(Index));
        }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> SendVerificationEmail(IndexViewModel model)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return View(model);
        //     }

        //     var user = await _userManager.GetUserAsync(User);
        //     if (user == null)
        //     {
        //         throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        //     }

        //     var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //     var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
        //     var email = user.Email;
        //     await _emailSender.SendEmailConfirmationAsync(email, callbackUrl);

        //     StatusMessage = "Verification email sent. Please check your email.";
        //     return RedirectToAction(nameof(Index));
        // }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                AddErrors(changePasswordResult);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.Information("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                AddErrors(addPasswordResult);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "Your password has been set.";

            return RedirectToAction(nameof(SetPassword));
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLogins()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ExternalLoginsViewModel { CurrentLogins = await _userManager.GetLoginsAsync(user) };
            model.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            model.ShowRemoveButton = false;//await _userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
            model.StatusMessage = StatusMessage;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(LinkLoginCallback));
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        public async Task<IActionResult> LinkLoginCallback()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(user.Id);
            if (info == null)
            {
                throw new ApplicationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await _userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            StatusMessage = "The external login was added.";
            return RedirectToAction(nameof(ExternalLogins));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred removing external login for user with ID '{user.Id}'.");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "The external login was removed.";
            return RedirectToAction(nameof(ExternalLogins));
        }

        [HttpGet]
        public async Task<IActionResult> TwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new TwoFactorAuthenticationViewModel
            {
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = user.TwoFactorEnabled,
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Disable2faWarning()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            return View(nameof(Disable2fa));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            _logger.Information("User with ID {UserId} has disabled 2fa.", user.Id);
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new EnableAuthenticatorViewModel();
            await LoadSharedKeyAndQrCodeUriAsync(user, model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }

            // Strip spaces and hypens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Code", "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            _logger.Information("User with ID {UserId} has enabled 2FA with an authenticator app.", user.Id);
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            TempData[RecoveryCodesKey] = recoveryCodes.ToArray();

            return RedirectToAction(nameof(ShowRecoveryCodes));
        }

        [HttpGet]
        public IActionResult ShowRecoveryCodes()
        {
            var recoveryCodes = (string[])TempData[RecoveryCodesKey];
            if (recoveryCodes == null)
            {
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            var model = new ShowRecoveryCodesViewModel { RecoveryCodes = recoveryCodes };
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetAuthenticatorWarning()
        {
            return View(nameof(ResetAuthenticator));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.Information("User with id '{UserId}' has reset their authentication app key.", user.Id);

            return RedirectToAction(nameof(EnableAuthenticator));
        }

        [HttpGet]
        public async Task<IActionResult> GenerateRecoveryCodesWarning()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' because they do not have 2FA enabled.");
            }

            return View(nameof(GenerateRecoveryCodes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            _logger.Information("User with ID {UserId} has generated new 2FA recovery codes.", user.Id);

            var model = new ShowRecoveryCodesViewModel { RecoveryCodes = recoveryCodes.ToArray() };

            return View(nameof(ShowRecoveryCodes), model);
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("Identity"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user, EnableAuthenticatorViewModel model)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            model.SharedKey = FormatKey(unformattedKey);
            model.AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey);
        }

        #endregion
    }
}