using GdevApps.DAL.DataContexts.AspNetUsers;
using GdevApps.DAL.Repositories.BaseRepository;

namespace GdevApps.DAL.Repositories.AspNetUserRepository
{
    public class AspNetUserRepository :
        EntityFrameworkRepository<AspNetUserContext>,
        IAspNetUserRepository
    {
        public AspNetUserRepository(AspNetUserContext context) :
            base(context)
        {
        }

        public AspNetUserRepository() :
            base(new AspNetUserContext())
        {
        }
    }
}
