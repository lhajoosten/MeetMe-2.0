using AutoMapper;
using AutoMapper.QueryableExtensions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Domain.Common;
using MeetMe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MeetMe.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation for read operations (CQRS Query side)
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    public class QueryRepository<T, TKey> : IQueryRepository<T, TKey> 
        where T : BaseEntity<TKey> 
        where TKey : IEquatable<TKey>
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly IMapper _mapper;

        public QueryRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _mapper = mapper;
        }

        #region Entity-based Methods

        public async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        }

        public IQueryable<T> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.FirstAsync(predicate, cancellationToken);
        }

        #endregion

        #region AutoMapper Projection Methods

        public async Task<TDto?> GetByIdProjectedAsync<TDto>(TKey id, CancellationToken cancellationToken = default) where TDto : class
        {
            return await _dbSet
                .Where(e => e.Id.Equals(id))
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TDto>> FindProjectedAsync<TDto>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where TDto : class
        {
            return await _dbSet
                .Where(predicate)
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TDto>> GetAllProjectedAsync<TDto>(CancellationToken cancellationToken = default) where TDto : class
        {
            return await _dbSet
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<TDto?> FirstOrDefaultProjectedAsync<TDto>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where TDto : class
        {
            return await _dbSet
                .Where(predicate)
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }

        #endregion
    }

    /// <summary>
    /// Convenience implementation for entities with int keys
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class QueryRepository<T> : QueryRepository<T, int>, IQueryRepository<T> 
        where T : BaseEntity<int>
    {
        public QueryRepository(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
