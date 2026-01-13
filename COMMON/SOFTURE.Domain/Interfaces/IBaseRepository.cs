using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using SOFTURE.Language.Common;

namespace SOFTURE.Domain.Interfaces;

public interface IBaseRepository<TEntity, in TIdentifier>
    where TEntity : class, IEntity, IAggregateRoot
    where TIdentifier : class, IIdentifier
{
    Task<Maybe<TEntity>> GetByIdAsync(TIdentifier id);
    Task<Maybe<IEnumerable<TEntity>>> GetAllAsync();
    Task<Result<TEntity>> AddAsync(TEntity entity);
    Task<Result<TEntity>> UpdateAsync(TEntity entity);
    Task<Result> RemoveAsync(TEntity entity);
    
    Task<Maybe<IEnumerable<TEntity>>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<Maybe<IEnumerable<TEntity>>> PaginateAsync(int page, int elements, Expression<Func<TEntity, bool>> predicate);
    Task<Result<int>> CountAsync();
}