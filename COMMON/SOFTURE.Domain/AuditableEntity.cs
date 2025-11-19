using SOFTURE.Domain.Interfaces;
using SOFTURE.Language.Common;

namespace SOFTURE.Domain
{
    public abstract class AuditableEntity<TIdentifier> : Entity<TIdentifier>, IAuditableEntity
        where TIdentifier : class, IIdentifier
    {
        public string? CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? Modified { get; set; }
    }
}
