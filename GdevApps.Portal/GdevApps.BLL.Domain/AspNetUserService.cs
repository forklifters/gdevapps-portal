using AutoMapper;
using GdevApps.BLL.Contracts;
using GdevApps.BLL.Models.AspNetUsers;
using GdevApps.BLL.Models.LicensedUser;
using GdevApps.DAL.Repositories.AspNetUserRepository;
using Serilog;
using System;
using System.Collections.Generic;
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


        public Task<IEnumerable<AspNetUserToken>> GetAllTokens()
        {
            var m = _aspNetUserRepository.GetAllAsync<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>().Result;
            return _mapper.Map<Task<IEnumerable<AspNetUserToken>>>(
                _aspNetUserRepository.GetAllAsync<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>());
        }

        public Task<IEnumerable<AspNetUser>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateUserTokensAsync(AspNetUserToken userTokens)
        {
            var userTokensModel = _mapper.Map<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(userTokens);
            _aspNetUserRepository.Update<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(userTokensModel);
            await _aspNetUserRepository.SaveAsync();
        }

        public Task<IEnumerable<AspNetUserToken>> GetAllTokensByUserIdAsync(string userId)
        {
            var models = _aspNetUserRepository.GetAsync<DAL.DataModels.AspNetUsers.AspNetUser.AspNetUserTokens>(filter: (u => u.UserId == userId));
            return _mapper.Map<Task<IEnumerable<AspNetUserToken>>>(models);
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
                var parentsDal = await _aspNetUserRepository.GetParentsByCreatorIdTempAsync(aspUserTeacherId);
                var parentsBll = _mapper.Map<List<ParentModel>>(parentsDal);

                return parentsBll;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}
