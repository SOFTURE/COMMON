using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using SOFTURE.Language.Common;

namespace SOFTURE.Domain.Interfaces;

public interface IReadBaseRepository<TEntity>
    where TEntity : class, IEntity
{
    Task<Maybe<IEnumerable<TEntity>>> GetAllAsync();
    Task<Maybe<IEnumerable<TEntity>>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<Maybe<IEnumerable<TEntity>>> PaginateAsync(int page, int elements, Expression<Func<TEntity, bool>> predicate);
    Task<Maybe<int>> CountAsync();
}