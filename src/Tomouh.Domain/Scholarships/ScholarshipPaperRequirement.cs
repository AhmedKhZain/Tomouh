using Common.BaseTypes;

namespace Tomouh.Domain.Scholarships;

public class ScholarshipPaperRequirement : AuditableEntity<Guid>
{
    public Guid ScholarshipId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsRequired { get; private set; }

    public ScholarshipPaperRequirement(
        string title,
        string? description = null,
        bool isRequired = true,
        Guid? createdBy = null)
        : base(Guid.NewGuid(), createdBy)
    {
        Title = title;
        Description = description;
        IsRequired = isRequired;
    }
}
