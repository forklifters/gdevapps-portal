using AutoMapper;
using GdevApps.BLL.Contracts;
using GdevApps.BLL.Models.AspNetUsers;
using GdevApps.BLL.Models.GDevSpreadSheetService;
using GdevApps.BLL.Models.LicensedUser;
using GdevApps.DAL.Repositories.AspNetUserRepository;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.BLL.Domain
{
    public class AspNetUserService : IAspNetUserService
    {
        private readonly IAspNetUserRepository _aspNetUserRepository;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public AspNetUserService(
            IAspNetUserRepository aspNetUserRepository,
            IMapper mapper,
            ILogger logger)
        {
            _aspNetUserRepository = aspNetUserRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<PortalUser>> GetAllUsersAsync()
        {
            _logger.Debug("GetAllUsersAsync was called");
            var aspUsers = (await _aspNetUserRepository.GetAllAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUsers>()).ToList();
            var portalUsers = new List<PortalUser>();
            _logger.Debug("{Number} asp users were found", aspUsers?.Count);
            foreach (var aspUser in aspUsers)
            {
                var portalUser = new PortalUser()
                {
                    Id = aspUser.Id,
                    Email = aspUser.Email,
                    UserName = aspUser.UserName
                };
                var roles = new List<PortalRole>();
                if (aspUser.ParentAspUser.Any())
                {
                    var parentsAspUser = aspUser.ParentAspUser.GroupBy(p => p.Id)
                                                                .Select(g => g.First())
                                                                .ToList();
                    foreach (var parent in parentsAspUser)
                    {
                        var role = new PortalRole()
                        {
                            RoleId = parent.Id,
                            Name = UserRoles.Parent,
                            UserId = parent.AspUser.Id,
                            CreatedByEmail = parent.CreatedByNavigation.Email,
                            CreatedById = parent.CreatedBy
                        };
                        roles.Add(role);
                    }
                }

                if (aspUser.TeacherAspNetUser.Any())
                {
                    var teacherAspUser = aspUser.TeacherAspNetUser.GroupBy(p => p.Id)
                                                                .Select(g => g.First())
                                                                .ToList();
                    foreach (var teacher in teacherAspUser)
                    {
                        var role = new PortalRole()
                        {
                            RoleId = teacher.Id,
                            Name = UserRoles.Teacher,
                            UserId = teacher.AspNetUser.Id,
                            CreatedByEmail = teacher.CreatedByNavigation.Email,
                            CreatedById = teacher.CreatedBy
                        };
                        roles.Add(role);
                    }
                }
                var adminRole = aspUser.AspNetUserRoles.Where(r => r.Role.Name == UserRoles.Admin).FirstOrDefault();
                if (adminRole != null)
                {
                    var role = new PortalRole()
                    {
                        RoleId = 0,
                        Name = UserRoles.Admin,
                        UserId = adminRole.UserId
                    };
                    roles.Add(role);
                }

                portalUser.Roles = roles;
                portalUsers.Add(portalUser);
            }

            return portalUsers;
        }

        public async Task UpdateUserTokensAsync(AspNetUserToken userTokens)
        {
            _logger.Debug("UpdateUserTokensAsync was called for user with id: {UserId}", userTokens.UserId);
            var userTokensModel = _mapper.Map<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(userTokens);
            _aspNetUserRepository.Update<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(userTokensModel);
            await _aspNetUserRepository.SaveAsync();
            _logger.Debug("Tokens were successfully updated for user with id: {UserId}", userTokens.UserId);
        }

        public async Task<IEnumerable<AspNetUserToken>> GetAllTokensByUserIdAsync(string userId)
        {
            _logger.Debug("GetAllTokensByUserIdAsync was called for user id {UserId}", userId);
            var models = await _aspNetUserRepository.GetAsync<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(filter: (u => u.UserId == userId));

            return _mapper.Map<IEnumerable<AspNetUserToken>>(models);
        }

        public IEnumerable<AspNetUserToken> GetAllTokensByUserId(string userId)
        {
            _logger.Debug("GetAllTokensByUserId was called for user id {UserId}", userId);
            var models = _aspNetUserRepository.Get<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(filter: (u => u.UserId == userId));

            return _mapper.Map<IEnumerable<AspNetUserToken>>(models);
        }

        public async Task<bool> AddParentAsync(Parent parent)
        {
            _logger.Debug("AddParentAsync was called for user {Email}, name {Name}", parent.Email, parent.Name);
            var model = _mapper.Map<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(parent);
            _aspNetUserRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(model);
            await _aspNetUserRepository.SaveAsync();
            _logger.Debug("Parent {Email} was successfully created", parent.Email);

            return true;
        }

        public async Task<bool> AddTeacherAsync(Teacher teacher)
        {
            _logger.Debug("AddTeacherAsync was called for user {Email}, name {Name}", teacher.Email, teacher.Name);
            var model = _mapper.Map<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>(teacher);
            _aspNetUserRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>(model);
            await _aspNetUserRepository.SaveAsync();
            _logger.Debug("Teacher {Email} was successfully created", teacher.Email);

            return true;
        }

        public async Task<Parent> GetParentByEmailAsync(string email)
        {
            _logger.Debug("GetParentByEmailAsync was called for email {Email}", email);
            var parentModel = await _aspNetUserRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(filter: f => f.Email == email);
            return _mapper.Map<Parent>(parentModel);
        }

        public async Task<Users> GetLicensedUserByEmailAsync(string email)
        {
            _logger.Debug("GetLicensedUserByEmailAsync was called for email {Email}", email);
            var userModel = await _aspNetUserRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.LicensedUser.Users>(filter: f => f.Email == email);
            return _mapper.Map<Users>(userModel);
        }

        public async Task<Teacher> GetTeacherByEmailAsync(string email)
        {
            _logger.Debug("GetTeacherByEmailAsync was called for email {Email}", email);
            var teacherModel = await _aspNetUserRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>(filter: f => f.Email == email);
            return _mapper.Map<Teacher>(teacherModel);
        }

        public bool AddUserLogin(AspNetUserLogin userLogin)
        {
            _logger.Debug("AddUserLogin was called for user with id {UserId}", userLogin.UserId);
            var dalUserLogin = _mapper.Map<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserLogins>(userLogin);
            _aspNetUserRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserLogins>(dalUserLogin);
            _logger.Debug("Login was successfully added for user with id {UserId}", userLogin.UserId);
            return true;
        }

        public async Task<bool> SetParentAspUserIdAsync(int parentId, string aspUserId)
        {
            _logger.Debug("SetParentAspUserIdAsync was called for parent id {ParentId} and asp user id {aspUserId}", parentId, aspUserId);
            var parentModel = await _aspNetUserRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(filter: f => f.Id == parentId);
            parentModel.AspUserId = aspUserId;
            _aspNetUserRepository.Update<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(parentModel);
            await _aspNetUserRepository.SaveAsync();
            _logger.Debug("Asp user with id {UserId} was successfully connected to parent {Email} with id {ParentId}", aspUserId, parentModel.Email, parentId);

            return true;
        }

        public async Task<bool> SetTeacherAspUserIdAsync(int teacherId, string aspUserId)
        {
            _logger.Debug("SetTeacherAspUserIdAsync was called for parent id {TeacherId} and asp user id {aspUserId}", teacherId, aspUserId);
            var teacherModel = await _aspNetUserRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>(filter: f => f.Id == teacherId);
            teacherModel.AspNetUserId = aspUserId;
            _aspNetUserRepository.Update<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>(teacherModel);
            await _aspNetUserRepository.SaveAsync();
            _logger.Debug("Asp user with id {UserId} was successfully connected to teacher {Email} with id {TeacherId}", aspUserId, teacherModel.Email, teacherId);

            return true;
        }

        public async Task<List<ParentModel>> GetAllParentsByTeacherAsync(string aspUserTeacherId)
        {
            _logger.Debug("GetAllParentsByTeacherAsync was called for teacher {TeacherAspUserId}", aspUserTeacherId);
            var parentsDal = (await _aspNetUserRepository.GetAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(filter: f => f.CreatedBy == aspUserTeacherId)).ToList();
            _logger.Debug("{Number} of parents were found", parentsDal?.Count);
            var parentsBll = new List<ParentModel>();
            foreach (var parent in parentsDal)
            {
                var parentModel = new ParentModel
                {
                    AspUserId = parent.AspUserId,
                    Avatar = parent.Avatar,
                    Email = parent.Email,
                    Id = parent.Id,
                    Name = parent.Name,
                    ParentSpreadsheets = new List<ParentSpreadsheet>()
                };
                foreach (var student in parent.ParentStudent)
                {
                    var parentGradebook = parent.ParentSharedGradeBook?.Where(p => p.ParentGradeBook.MainGradeBookId == student.GradeBook.Id).Select(p => p.ParentGradeBook).FirstOrDefault();
                    var parentSpreadsheetInfo = new ParentSpreadsheet();
                    if (parentGradebook != null)
                    {
                        parentSpreadsheetInfo = _mapper.Map<ParentSpreadsheet>(parentGradebook);
                    }

                    parentSpreadsheetInfo.StudentEmail = student.StudentEmail;
                    parentModel.ParentSpreadsheets.Add(parentSpreadsheetInfo);
                }

                parentsBll.Add(parentModel);
            }

            return parentsBll;
        }

        public async Task<bool> DeleteParentByIdAsync(int id)
        {
            try
            {
                _logger.Debug("DeleteParentByIdAsync was called for parent {ParentId}", id);
                await _aspNetUserRepository.DeleteAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(id);
                await _aspNetUserRepository.SaveAsync();
                _logger.Debug("Parent with id {ParentId} was successfully deleted", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Parent with id: {ParentId} was not deleted", id);
                return false;
            }
        }

        public async Task<bool> DeleteTeacherByIdAsync(int id)
        {
            try
            {
                _logger.Debug("DeleteTeacherByIdAsync was called for parent {TeacherId}", id);
                await _aspNetUserRepository.DeleteAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>(id);
                await _aspNetUserRepository.SaveAsync();
                _logger.Debug("Teacher with id {TeacherId} was successfully deleted", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Teacher with id: {TeacherId} was not deleted", id);
                return false;
            }
        }

        public async Task<Parent> GetParentByIdAsync(int id)
        {
            _logger.Debug("GetParentByIdAsync was called for id {Id}", id);
            var parentDal = await _aspNetUserRepository.GetByIdAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(id);
            return _mapper.Map<Parent>(parentDal);
        }

        public async Task<Teacher> GetTeacherByIdAsync(int id)
        {
            _logger.Debug("GetTeacherByIdAsync was called for id {Id}", id);
            var teacher = await _aspNetUserRepository.GetByIdAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>(id);
            return _mapper.Map<Teacher>(teacher);
        }

        public async Task<List<Parent>> GetAllParentsAsync()
        {
            _logger.Debug("GetAllParentsAsync was called");
            var parents = (await _aspNetUserRepository.GetAllAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>()).ToList();
            _logger.Debug("{Number} of parents were found", parents.Count);

            return _mapper.Map<List<Parent>>(parents);
        }

        public async Task<List<Teacher>> GetAllTeachersAsync()
        {
            _logger.Debug("GetAllTeachersAsync was called");
            var teachers = (await _aspNetUserRepository.GetAllAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>()).ToList();
            _logger.Debug("{Number} of teachers were found", teachers.Count);

            return _mapper.Map<List<Teacher>>(teachers);
        }
    }
}
