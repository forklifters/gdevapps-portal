using Microsoft.EntityFrameworkCore;
using GdevApps.DAL.DataModels.GradeBook;
using GdevApps.DAL.DataContexts.Gradebook.Config;

namespace GdevApps.DAL.DataContexts.GradeBook
{
    public class GradeBookContext : DbContext
    {
        public GradeBookContext(DbContextOptions<GradeBookContext> options) :
            base(options)
        { }

        public GradeBookContext() { }

        public virtual DbSet<Folder> Folder { get; set; }
        public virtual DbSet<FolderType> FolderType { get; set; }
        public virtual DbSet<GdevApps.DAL.DataModels.GradeBook.GradeBook> GradeBook { get; set; }
        public virtual DbSet<ParentGradeBook> ParentGradeBook { get; set; }
        public virtual DbSet<ParentSharedGradeBook> ParentSharedGradeBook { get; set; }
        public virtual DbSet<SharedStatus> SharedStatus { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
           builder.ApplyConfiguration(new FolderConfig());
           builder.ApplyConfiguration(new FolderTypeConfig());
           builder.ApplyConfiguration(new GradeBookConfig());
           builder.ApplyConfiguration(new ParentGradeBookConfig());
           builder.ApplyConfiguration(new ParentSharedGradeBookConfig());
           builder.ApplyConfiguration(new SharedStatusConfig());
        }
    }
}