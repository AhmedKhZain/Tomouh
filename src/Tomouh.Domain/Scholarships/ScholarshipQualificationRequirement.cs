using Common.BaseTypes;

namespace Tomouh.Domain.Scholarships;

public class ScholarshipQualificationRequirement : AuditableEntity<Guid>
{
    public Guid ScholarshipId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public bool IsRequired { get; private set; }

    public ScholarshipQualificationRequirement(
        string title,
        string description,
        bool isRequired = true,
        Guid? createdBy = null)
        : base(Guid.NewGuid(), createdBy)
    {
        Title = title;
        Description = description;
        IsRequired = isRequired;
    }
}