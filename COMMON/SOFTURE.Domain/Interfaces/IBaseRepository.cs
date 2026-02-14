using CSharpFunctionalExtensions;
using SOFTURE.Language.Common;

namespace SOFTURE.Domain.Interfaces;

public interface IBaseRepository<TEntity, in TIdentifier>
    where TEntity : class, IEntity, IAggregateRoot
    where TIdentifier : class, IIdentifier
{
    Task<Result<TEntity>> GetAsync(TIdentifier id);
    Task<Result<TEntity>> AddAsync(TEntity entity);
    Task<Result<TEntity>> UpdateAsync(TEntity entity);
    Task<Result> RemoveAsync(TEntity entity);
}