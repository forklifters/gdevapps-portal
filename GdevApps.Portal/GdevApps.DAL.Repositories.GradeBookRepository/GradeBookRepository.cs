using GdevApps.DAL.DataContexts.AspNetUsers;
using GdevApps.DAL.Repositories.BaseRepository;

namespace GdevApps.DAL.Repositories.GradeBookRepository
{
    public class GradeBookRepository :  EntityFrameworkRepository<AspNetUserContext>,
        IGradeBookRepository 
    {

         public GradeBookRepository(AspNetUserContext context) :
            base(context)
        {
        }

        public GradeBookRepository() :
            base(new AspNetUserContext())
        {
        }
    }
}
