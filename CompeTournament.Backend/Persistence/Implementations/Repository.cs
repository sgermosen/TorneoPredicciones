namespace CompeTournament.Backend.Persistence.Implementations
{
    using CompeTournament.Backend.Data;
    using CompeTournament.Backend.Helpers;
    using Contracts;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class, IBaseEntity
    {
        public readonly ApplicationDbContext Context;

        public Repository(ApplicationDbContext context)
        {
            Context = context;
        }

        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            await Context.Set<TEntity>().AddAsync(entity);
            await SaveAllAsync();
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            Context.Set<TEntity>().Update(entity);
            await SaveAllAsync();
            return entity;
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            //Mapper.Initialize(cfg => cfg.CreateMap<TEntity, TEntity>());
            ////or
            //var config = new MapperConfiguration(cfg => cfg.CreateMap<TEntity, TEntity>());
            //var mapper = config.CreateMapper();
            // or


            Context.Add(entity);
            await Context.SaveChangesAsync();
            return entity;
        }
               
        public async Task<bool> DeleteAsync(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
            int result = await Context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> ExistAsync(int id)
        {
            return await Context.Set<TEntity>().AnyAsync(e => e.Id == id);
        }

        public bool Exists(int key)
        {
            return Context.Set<TEntity>().Any(e => e.Id == key);
        }

        public Task<List<TEntity>> FindByClause(Func<TEntity, bool> selector = null)
        {
            var models = Context.Set<TEntity>()
                .Where(selector ?? (s => true));

            return Task.Run(() => models.ToList());
        }

        public async Task<TEntity> FindByIdAsync(int key)
        {
            var entity = await Context.Set<TEntity>().FindAsync(key);
            return entity;
        }

        public IQueryable<TEntity> GetAll()
        {
            // return this.context.Set<T>().AsNoTracking();
            return Context.Set<TEntity>().AsNoTracking();
        }

        public Task<TEntity> GetByClause(Func<TEntity, bool> selector = null)
        {
            var models = Context.Set<TEntity>()
                .Where(selector ?? (s => true));

            return Task.Run(() => models.FirstOrDefault());
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            var entity = await Context.Set<TEntity>().FindAsync(id);
            return entity;
        }

        public async Task<bool> SaveAllAsync()
        {
            return await Context.SaveChangesAsync() > 0;
        }
        //public async Task<bool> UpdateAsync(TEntity entity)
        //{
        //    _context.Update(entity);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //    return true;
        //}
    }
}
