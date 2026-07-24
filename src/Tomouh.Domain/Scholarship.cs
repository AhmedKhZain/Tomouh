using Common.BaseTypes;

namespace Tomouh.Domain;

public class Scholarship : AuditableEntity<Guid>
{
    public Guid FundOrganizationId { get; private set; }
    public FundOrganization FundOrganization { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? Content { get; private set; }
}
