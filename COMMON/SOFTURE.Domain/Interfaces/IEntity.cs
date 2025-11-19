namespace SOFTURE.Domain.Interfaces;

public interface IEntity
{
    IReadOnlyList<IDomainEvents> GetEvents();
    void ClearEvents();
}