using Common.BaseTypes;

namespace Tomouh.Domain;

public class FundOrganization : AuditableEntity<Guid>
{
    public string Name { get; private set; }
    public int? YearlyStudentCount { get; private set; }
    public string? Description { get; private set; }
    public FundOrgType FundOrgType { get; private set; }
    public FundOrganization(
        string name,
        int? yearlyStudentCount,
        string? description,
        FundOrgType fundOrgType,
        Guid? createdBy = null)
        : base(Guid.NewGuid(), createdBy)
    {
        Name = name;
        YearlyStudentCount = yearlyStudentCount;
        Description = description;
        FundOrgType = fundOrgType;
    }
    public void Update(
        string? name = null,
        int? yearlyStudentCount = null,
        string? description = null,
        FundOrgType? fundOrgType = null)
    {
        MarkUpdated();
        Name = name ?? Name;
        Description = description ?? Description;
        YearlyStudentCount = yearlyStudentCount ?? YearlyStudentCount;
        FundOrgType = fundOrgType ?? FundOrgType;
    }
}
