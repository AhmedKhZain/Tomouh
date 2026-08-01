using Common.BaseTypes;
using Common.DataConvrters;

namespace Tomouh.Domain.Funders;

public class Funder : AuditableAggregateRoot<Guid>
{
    public string Name { get; private set; }
    public int? YearlyStudentCount { get; private set; }
    public string? Description { get; private set; }
    public FundOrgType FundOrgType { get; private set; }
    public Funder(
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
[StoreEnumAsString(maxLength: 20, namingStrategy: EnumNamingStrategy.Default, caseInsensitive: false)]
public enum FundOrgType
{
    Country = 1,
    University,
    EuropeUnion
}
