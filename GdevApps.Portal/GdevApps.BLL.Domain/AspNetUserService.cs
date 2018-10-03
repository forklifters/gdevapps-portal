using AutoMapper;
using GdevApps.BLL.Contracts;
using GdevApps.BLL.Models.AspNetUsers;
using GdevApps.DAL.Repositories.AspNetUserRepository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GdevApps.BLL.Domain
{
    public class AspNetUserService : IAspNetUserService
    {
        private readonly IAspNetUserRepository _aspNetUserRepository;
        private readonly IMapper _mapper;

        public AspNetUserService(
            IAspNetUserRepository aspNetUserRepository,
            IMapper mapper)
        {
            _aspNetUserRepository = aspNetUserRepository;
            _mapper = mapper;
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
    }
}
