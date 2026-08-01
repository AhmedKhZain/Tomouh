using Common.BaseTypes;

namespace Tomouh.Domain.Scholarships.Events;

#region Paper Requirement Events

public record ScholarshipPaperRequirementAddedIntegrationEvent(
    Guid ScholarshipId,
    Guid PaperRequirementId,
    string Title,
    bool IsRequired
) : IIntegrationEvent;

public record ScholarshipPaperRequirementRemovedIntegrationEvent(
    Guid ScholarshipId,
    Guid PaperRequirementId
) : IIntegrationEvent;

#endregion

#region Qualification Requirement Events

public record ScholarshipQualificationRequirementAddedIntegrationEvent(
    Guid ScholarshipId,
    Guid QualificationRequirementId,
    string Title,
    bool IsRequired
) : IIntegrationEvent;

public record ScholarshipQualificationRequirementRemovedIntegrationEvent(
    Guid ScholarshipId,
    Guid QualificationRequirementId
) : IIntegrationEvent;

#endregion