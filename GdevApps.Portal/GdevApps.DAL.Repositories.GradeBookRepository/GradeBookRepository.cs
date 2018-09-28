using GdevApps.DAL.DataContexts.GradeBook;
using GdevApps.DAL.Repositories.BaseRepository;

namespace GdevApps.DAL.Repositories.GradeBookRepository
{
    public class GradeBookRepository :  EntityFrameworkRepository<GradeBookContext>,
        IGradeBookRepository 
    {

         public GradeBookRepository(GradeBookContext context) :
            base(context)
        {
        }

        public GradeBookRepository() :
            base(new GradeBookContext())
        {
        }
    }
}
