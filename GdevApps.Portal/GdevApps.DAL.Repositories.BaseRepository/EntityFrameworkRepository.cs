using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdevApps.DAL.Repositories.BaseRepository
{
    public class EntityFrameworkRepository<TContext> :
         EntityFrameworkReadOnlyRepository<TContext>, IRepository where TContext : DbContext
    {
        public EntityFrameworkRepository(TContext context)
            : base(context)
        {
        }

        public virtual void Create<TEntity>(TEntity entity, string createdBy = null)
            where TEntity : class
        {

            context.Set<TEntity>().Add(entity);
        }

        public virtual void Update<TEntity>(TEntity entity, string modifiedBy = null)
            where TEntity : class
        {
            var local = context.Set<TEntity>().Local.Where(entry => entry.Equals(entity)).FirstOrDefault();
            if (local != null) 
            {
                // detach
                context.Entry(local).State = EntityState.Detached;
            }

            context.Set<TEntity>().Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete<TEntity>(object id)
            where TEntity : class
        {
            TEntity entity = context.Set<TEntity>().Find(id);
            Delete(entity);
        }

        public virtual async Task DeleteAsync<TEntity>(object id)
            where TEntity : class
        {
            TEntity entity = await context.Set<TEntity>().FindAsync(id);
            Delete(entity);
        }

        public virtual void Delete<TEntity>(TEntity entity)
            where TEntity : class
        {
            var dbSet = context.Set<TEntity>();
            if (context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
            dbSet.Remove(entity);
        }

        public virtual void Save()
        {
            context.SaveChanges();
        }
        public virtual Task SaveAsync()
        {
            return context.SaveChangesAsync();
        }
    }
}
