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


        public async Task<IEnumerable<AspNetUserToken>> GetAllTokens()
        {
            try
            {
                var allTokens = await _aspNetUserRepository.GetAllAsync<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>();
                return _mapper.Map<IEnumerable<AspNetUserToken>>(allTokens);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<IEnumerable<PortalUser>> GetAllUsersAsync()
        {
           try
            {
                var aspUsers = (await _aspNetUserRepository.GetAllAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUsers>()).ToList();
                var portalUsers = new List<PortalUser>();
                foreach(var aspUser in aspUsers)
                {
                    var portalUser = new PortalUser()
                    {
                        Id = aspUser.Id,
                        Email = aspUser.Email,
                        UserName = aspUser.UserName
                    };
                    var roles = new List<PortalRole>();
                    if(aspUser.ParentAspUser.Any())
                    {
                        foreach (var parent in aspUser.ParentAspUser)
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
                        foreach (var teacher in aspUser.TeacherAspNetUser)
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
                    portalUser.Roles = roles;
                    portalUsers.Add(portalUser);
                }

                return portalUsers;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task UpdateUserTokensAsync(AspNetUserToken userTokens)
        {
            try
            {
                var userTokensModel = _mapper.Map<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(userTokens);
                _aspNetUserRepository.Update<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(userTokensModel);
                await _aspNetUserRepository.SaveAsync();
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<IEnumerable<AspNetUserToken>> GetAllTokensByUserIdAsync(string userId)
        {
            try
            {
                var models = await _aspNetUserRepository.GetAsync<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(filter: (u => u.UserId == userId));
                return _mapper.Map<IEnumerable<AspNetUserToken>>(models);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

           public IEnumerable<AspNetUserToken> GetAllTokensByUserId(string userId)
        {
            try
            {
                var models = _aspNetUserRepository.Get<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(filter: (u => u.UserId == userId));
                return _mapper.Map<IEnumerable<AspNetUserToken>>(models);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
       
        public bool AddParent(Parent parent)
        {
             try
            {
                var model = _mapper.Map<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(parent);
                _aspNetUserRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(model);
                _aspNetUserRepository.Save();
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<Parent> GetParentByEmailAsync(string email)
        {
            try
            {
                var parentModel = await _aspNetUserRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(filter: f => f.Email == email);
                return _mapper.Map<Parent>(parentModel);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<Users> GetLicensedUserByEmailAsync(string email)
        {
            try
            {
                var userModel = await _aspNetUserRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.LicensedUser.Users>(filter: f => f.Email == email);
                return _mapper.Map<Users>(userModel);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<Teacher> GetTeacherByEmailAsync(string email)
        {
            try
            {
                var teacherModel = await _aspNetUserRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>(filter: f => f.Email == email);
                return _mapper.Map<Teacher>(teacherModel);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public bool AddUserLogin(AspNetUserLogin userLogin)
        {
             try
            {   
                var dalUserLogin = _mapper.Map<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserLogins>(userLogin);
                _aspNetUserRepository.Create<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserLogins>(dalUserLogin);
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<bool> SetParentAspUserId(int parentId, string aspUserId)
        {
            try
            {
                var parentModel = await _aspNetUserRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(filter: f => f.Id == parentId);
                parentModel.AspUserId = aspUserId;
                _aspNetUserRepository.Update<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(parentModel);
                _aspNetUserRepository.Save();
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<bool> SetTeacherAspUserId(int teacherId, string aspUserId)
        {
            try
            {
                var teacherModel = await _aspNetUserRepository.GetFirstAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>(filter: f => f.Id == teacherId);
                teacherModel.AspNetUserId = aspUserId;
                _aspNetUserRepository.Update<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Teacher>(teacherModel);
                _aspNetUserRepository.Save();
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<List<ParentModel>> GetAllParentsByTeacherAsync(string aspUserTeacherId)
        {
            try
            {
                var parentsDal = (await _aspNetUserRepository.GetAsync<GdevApps.DAL.DataModels.AspNetUsers.AspNetUser.Parent>(filter: f=>f.CreatedBy == aspUserTeacherId)).ToList();
                var parentsBll = new List<ParentModel>();
                foreach(var parent in parentsDal){
                    var parentModel = new ParentModel{
                        AspUserId = parent.AspUserId,
                        Avatar = parent.Avatar,
                        Email = parent.Email,
                        Id = parent.Id,
                        Name = parent.Name,
                        ParentSpreadsheets = new List<ParentSpreadsheet>()
                    };
                    foreach(var student in parent.ParentStudent)
                    {
                        var parentGradebook = parent.ParentSharedGradeBook?.Where(p=>p.ParentGradeBook.MainGradeBookId == student.GradeBook.Id).Select(p => p.ParentGradeBook).FirstOrDefault();
                        var parentSpreadsheetInfo = new ParentSpreadsheet();
                        if(parentGradebook != null){
                            parentSpreadsheetInfo = _mapper.Map<ParentSpreadsheet>(parentGradebook);
                        }

                        parentSpreadsheetInfo.StudentEmail = student.StudentEmail;
                        parentModel.ParentSpreadsheets.Add(parentSpreadsheetInfo);
                    }

                    parentsBll.Add(parentModel);
                }


                return parentsBll;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public bool DeleteParentById(int id)
        {
            try
            {
                _aspNetUserRepository.Delete<Parent>(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Parent with id: {ParentId} was not deleted", id);
                return false;
            }
        }

        public bool DeleteTeacherById(int id)
        {
            try
            {
                _aspNetUserRepository.Delete<Teacher>(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Teacher with id: {TeacherId} was not deleted", id);
                return false;
            }
        }
    }
}
