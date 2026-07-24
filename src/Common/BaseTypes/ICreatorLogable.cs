using Common.AuditLogs;

namespace Common.BaseTypes;

/// <summary>
/// Defines a contract for entities that track their creator.
/// </summary>
public interface ICreatorTrackable
{
    /// <summary>
    /// Gets the unique identifier of the user who created the entity.
    /// </summary>
    Guid? CreatedBy { get; }

    /// <summary>
    /// Sets the creator's unique identifier.
    /// </summary>
    /// <param name="creatorId">The GUID of the creator.</param>
    void SetCreator(Guid creatorId);
}
/// <summary>
/// Defines a contract for entities that track their creator and the Creator can be the System.
/// </summary>
public interface IMultyWayCreatableTrackable
{
    /// <summary>
    /// Gets the unique identifier of the user who created the entity.
    /// </summary>
    Guid? CreatedBy { get; }

    /// <summary>
    /// Sets the creator's unique identifier.
    /// </summary>
    /// <param name="creatorId">The GUID of the creator.</param>
    /// <param name="actorType">The Type of the creator.
    /// can be User or System</param>
    void SetCreator(Guid? creatorId, CreationActorType actorType = CreationActorType.User);
}