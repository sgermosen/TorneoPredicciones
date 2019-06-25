namespace CompeTournament.Backend.Persistence.Contracts
{
    using CompeTournament.Backend.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IRepository<TEntity>
        where TEntity : class, IBaseEntity
    {
        IQueryable<TEntity> GetAll();

        Task<TEntity> GetByIdAsync(int id);

        // Task CreateAsync(TEntity entity);
        Task<bool> ExistAsync(int id);
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> CreateAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(TEntity entity);
        // Task<bool> UpdateAsync(TEntity entity);
        bool Exists(int key);
        Task<TEntity> FindByIdAsync(int key);//will find by id and return the entity
        Task<List<TEntity>> FindByClause(Func<TEntity, bool> selector = null);
        Task<TEntity> GetByClause(Func<TEntity, bool> selector = null);
        //IQueryable<TEntity> GetAll();
        //Task<TEntity> GetById(int id);
        //Task Create(TEntity entity);
        //Task Update(int id, TEntity entity);
        //Task Delete(int id);
    }
}
