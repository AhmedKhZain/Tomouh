using Common.AuditLogs;
using Common.BaseTypes;
using Common.ResultOf;
using Tomouh.Domain.Common.Events;
using Tomouh.Domain.Scholarships.Events;

namespace Tomouh.Domain.Scholarships;

public class Scholarship : AuditableAggregateRoot<Guid>
{
    public static readonly string NameOfScholarship = typeof(Scholarship).Name;
    public static readonly string NameOfPaperRequirement = $"{NameOfScholarship}:{typeof(ScholarshipPaperRequirement).Name}";
    public static readonly string NameOfQualificationRequirement = $"{NameOfScholarship}:{typeof(ScholarshipQualificationRequirement).Name}";

    public Guid FunderId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public string? Content { get; private set; }

    private readonly List<ScholarshipPaperRequirement> _papersNeeded = new();
    public IReadOnlyCollection<ScholarshipPaperRequirement> PapersNeeded => _papersNeeded.AsReadOnly();

    private readonly List<ScholarshipQualificationRequirement> _qualificationsNeeded = new();
    public IReadOnlyCollection<ScholarshipQualificationRequirement> QualificationsNeeded => _qualificationsNeeded.AsReadOnly();

    public Scholarship(
        Guid funderId,
        string name,
        string? description = null,
        string? content = null,
        Guid? createdBy = null)
        : base(Guid.NewGuid(), createdBy)
    {
        FunderId = funderId;
        Name = name;
        Description = description;
        Content = content;
    }

    private Scholarship() : base() { }

    #region Paper Requirements Management

    /// <summary>
    /// Adds a new paper requirement to the scholarship and records an audit log event.
    /// </summary>
    public ResultOf<ScholarshipPaperRequirement> AddPaperRequirement(
        string title,
        string? description,
        bool isRequired,
        Guid executedByUserId)
    {
        var paper = new ScholarshipPaperRequirement(title, description, isRequired, createdBy: executedByUserId);
        _papersNeeded.Add(paper);

        MarkUpdated();

        var audit = AuditLog.Create(
            originalState: paper,
            action: AuditActionType.Create,
            editedEntityName: NameOfPaperRequirement,
            customEntityId: $"{Id}_{paper.Id}"
        );
        audit.SetCreator(executedByUserId);

        AddIntegrationEvent(new AuditLogedEvent(audit));
        AddIntegrationEvent(new ScholarshipPaperRequirementAddedIntegrationEvent(
            ScholarshipId: Id,
            PaperRequirementId: paper.Id,
            Title: paper.Title,
            IsRequired: paper.IsRequired
        ));

        return paper;
    }

    /// <summary>
    /// Removes a paper requirement by ID and records an audit log event.
    /// </summary>
    public ResultOf<Done> RemovePaperRequirement(Guid paperRequirementId, Guid executedByUserId)
    {
        var paper = _papersNeeded.FirstOrDefault(p => p.Id == paperRequirementId);
        if (paper is null)
        {
            return ScholarshipErrors.PaperRequirementNotFound;
        }

        var audit = AuditLog.Create(
            originalState: paper,
            action: AuditActionType.Delete,
            editedEntityName: NameOfPaperRequirement,
            customEntityId: $"{Id}_{paper.Id}"
        );
        audit.SetCreator(executedByUserId);

        _papersNeeded.Remove(paper);
        MarkUpdated();

        AddIntegrationEvent(new AuditLogedEvent(audit));
        AddIntegrationEvent(new ScholarshipPaperRequirementRemovedIntegrationEvent(
            ScholarshipId: Id,
            PaperRequirementId: paperRequirementId
        ));

        return Done.Default;
    }

    #endregion

    #region Qualification Requirements Management

    /// <summary>
    /// Adds a new qualification requirement to the scholarship and records an audit log event.
    /// </summary>
    public ResultOf<ScholarshipQualificationRequirement> AddQualificationRequirement(
        string title,
        string description,
        bool isRequired,
        Guid executedByUserId)
    {
        var qualification = new ScholarshipQualificationRequirement(title, description, isRequired, createdBy: executedByUserId);
        _qualificationsNeeded.Add(qualification);

        MarkUpdated();

        var audit = AuditLog.Create(
            originalState: qualification,
            action: AuditActionType.Create,
            editedEntityName: NameOfQualificationRequirement,
            customEntityId: $"{Id}_{qualification.Id}"
        );
        audit.SetCreator(executedByUserId);

        AddIntegrationEvent(new AuditLogedEvent(audit));
        AddIntegrationEvent(new ScholarshipQualificationRequirementAddedIntegrationEvent(
            ScholarshipId: Id,
            QualificationRequirementId: qualification.Id,
            Title: qualification.Title,
            IsRequired: qualification.IsRequired
        ));

        return qualification;
    }

    /// <summary>
    /// Removes a qualification requirement by ID and records an audit log event.
    /// </summary>
    public ResultOf<Done> RemoveQualificationRequirement(Guid qualificationRequirementId, Guid executedByUserId)
    {
        var qualification = _qualificationsNeeded.FirstOrDefault(q => q.Id == qualificationRequirementId);
        if (qualification is null)
        {
            return ScholarshipErrors.QualificationRequirementNotFound;
        }

        var audit = AuditLog.Create(
            originalState: qualification,
            action: AuditActionType.Delete,
            editedEntityName: NameOfQualificationRequirement,
            customEntityId: $"{Id}_{qualification.Id}"
        );
        audit.SetCreator(executedByUserId);

        _qualificationsNeeded.Remove(qualification);
        MarkUpdated();

        AddIntegrationEvent(new AuditLogedEvent(audit));
        AddIntegrationEvent(new ScholarshipQualificationRequirementRemovedIntegrationEvent(
            ScholarshipId: Id,
            QualificationRequirementId: qualificationRequirementId
        ));

        return Done.Default;
    }

    #endregion
}