using Common.BaseTypes;

namespace Tomouh.Domain.UserInterests;

public class UserInterest : AuditableAggregateRoot<Guid>
{
    public Guid UserId { get; set; }
    public Guid ScholarshipId { get; set; }
}
