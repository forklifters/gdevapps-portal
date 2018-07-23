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
            return _mapper.Map<Task<IEnumerable<AspNetUserToken>>>(
                _aspNetUserRepository.GetAllAsync<DAL.DataModels.AspNetUsers.AspNetUserTokens>());
        }

        public Task<IEnumerable<AspNetUser>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }
    }
}
