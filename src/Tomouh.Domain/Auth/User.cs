using Common.AuditLogs;
using Common.BaseTypes;
using Common.Enums;
using Common.Markups;
using Common.ResultOf;
using Common.Services;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tomouh.Domain.Auth.Events;

namespace Tomouh.Domain.Auth;

public class User : AuditableAggregateRoot<Guid>, IHasCustomSerializationOptions
{
    public static readonly string NameOfUser = typeof(User).Name;
    public static readonly string NameOfUserProfile = $"{NameOfUser}:{typeof(UserProfile).Name}";

    public Name Name { get; private set; } = null!;
    public TFAStatus TFA { get; private set; } = null!;
    public ConfirmedEmail Email { get; private set; } = null!;
    public AccountStatus Status { get; private set; } = null!;

    // Computed property combining FirstName and LastName dynamically
    public string FullName => $"{Name.FirstName} {Name.LastName}".Trim();
    public string ShowName => Name.ShowName;


    private string _passwordHash = null!;


    private readonly List<UserProfile> _profiles = new();
    public IReadOnlyCollection<UserProfile> Profiles => _profiles.AsReadOnly();

    public User(
        string showName,
        string firstName,
        string lastName,
        string email)
        : base(Guid.NewGuid(), null)
    {
        Name = new Name(showName, firstName, lastName);
        Email = new ConfirmedEmail(email);
        TFA = new TFAStatus();
        Status = new AccountStatus();
        _profiles = new List<UserProfile>();
        AddProfile(Role.User, Id);
    }

    private User() : base() { }


    #region Json 
    [BsonConstructor]
    [JsonConstructor]
    private User(
    Guid id,
    Name name,
    ConfirmedEmail email,
    TFAStatus tfa,
    AccountStatus status,
    string passwordHash,
    List<UserProfile> profiles,
    DateTime? lastUpdate,
    DateTime createdAt,
    Guid? createdBy) : base(id, null)
    {
        Name = name;
        Email = email;
        TFA = tfa;
        Status = status;
        _passwordHash = passwordHash;
        _profiles = profiles ?? new List<UserProfile>();
        CreatedAt = createdAt;
        LastUpdate = lastUpdate;
        CreatedBy = createdBy;
    }
    public JsonSerializerOptions Options => CustomOptions;

    public static readonly JsonSerializerOptions CustomOptions = CreateJsonOptions();

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        options.Converters.Add(new SmartEnumJsonConverter<Role>());

        return options;
    }
    #endregion

    #region Profile Management

    /// <summary>
    /// Adds a new user profile for a specific role and triggers a creation audit log event via integration messaging.
    /// </summary>
    /// <param name="role">The role associated with the new profile.</param>
    /// <param name="executedByUserId">The unique identifier of the user performing this action.</param>
    /// <returns>A result indicating success (Done) or an error if the profile already exists or audit logging fails.</returns>
    public ResultOf<Done> AddProfile(Role role, Guid executedByUserId)
    {
        if (_profiles.Any(p => p.Role == role))
        {
            return UserErrors.ProfileAlreadyExists;
        }

        var newProfile = new UserProfile(role, createdBy: executedByUserId);
        _profiles.Add(newProfile);
        newProfile.MarkUpdated();
        IsUpdated = true;

        var audit = AuditLog.Create(
            originalState: newProfile,
            action: AuditActionType.Create,
            editedEntityName: NameOfUserProfile,
            customEntityId: $"{this.Id}_{role.NormalizedLowerCaseName}"
        );

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));

        return Done.Default;
    }
    /// <summary>
    /// Grants a specific permission to an existing profile and triggers an update audit log event.
    /// </summary>
    /// <param name="role">The role associated with the profile.</param>
    /// <param name="permission">The permission string to be granted.</param>
    /// <param name="executedByUserId">The unique identifier of the user performing this action.</param>
    /// <returns>A result indicating success (Done) or an error if the profile is not found or permission already exists.</returns>
    public ResultOf<Done> GrantPermissionToProfile(Role role, string permission, Guid executedByUserId)
    {
        var profile = _profiles.FirstOrDefault(p => p.Role == role);
        if (profile is null) return UserErrors.ProfileNotFound;
        if (profile.HasPermission(permission)) return UserErrors.PermissionAlreadyExists;

        var audit = AuditLog.Create(
            originalState: profile,
            action: AuditActionType.Update,
            editedEntityName: NameOfUserProfile,
            customEntityId: $"{this.Id}_{role.NormalizedLowerCaseName}"
        );

        profile.GrantPermission(permission);

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        profile.MarkUpdated();
        this.MarkUpdated();

        return Done.Default;
    }

    /// <summary>
    /// Revokes a specific permission from an existing profile and triggers an update audit log event.
    /// </summary>
    /// <param name="role">The role associated with the profile.</param>
    /// <param name="permission">The permission string to be revoked.</param>
    /// <param name="executedByUserId">The unique identifier of the user performing this action.</param>
    /// <returns>A result indicating success (Done) or an error if the profile or permission is not found.</returns>
    public ResultOf<Done> RevokePermissionFromProfile(Role role, string permission, Guid executedByUserId)
    {
        var profile = _profiles.FirstOrDefault(p => p.Role == role);
        if (profile is null) return UserErrors.ProfileNotFound;
        if (!profile.HasPermission(permission)) return UserErrors.PermissionDoesNotExist;

        var audit = AuditLog.Create(
            originalState: profile,
            action: AuditActionType.Update,
            editedEntityName: NameOfUserProfile,
            customEntityId: $"{this.Id}_{role.NormalizedLowerCaseName}"
        );

        profile.RevokePermission(permission);

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        profile.MarkUpdated();
        this.MarkUpdated();

        return Done.Default;
    }

    /// <summary>
    /// Adds or updates a metadata key-value pair for an existing profile and triggers an update audit log event.
    /// </summary>
    /// <param name="role">The role associated with the profile.</param>
    /// <param name="key">The metadata key configuration entry.</param>
    /// <param name="value">The metadata value payload linked to the entry key.</param>
    /// <param name="executedByUserId">The unique identifier of the user performing this action.</param>
    /// <returns>A result indicating success (Done) or an error if the profile is not found.</returns>
    public ResultOf<Done> AddOrUpdateProfileMetadata(Role role, string key, string value, Guid executedByUserId)
    {
        var profile = _profiles.FirstOrDefault(p => p.Role == role);
        if (profile is null) return UserErrors.ProfileNotFound;

        var audit = AuditLog.Create(
            originalState: profile,
            action: AuditActionType.Update,
            editedEntityName: NameOfUserProfile,
            customEntityId: $"{this.Id}_{role.NormalizedLowerCaseName}"
        );

        profile.AddOrUpdateMetadata(key, value);

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        profile.MarkUpdated();
        this.MarkUpdated();

        return Done.Default;
    }

    /// <summary>
    /// Removes a metadata key from an existing profile and triggers an update audit log event.
    /// </summary>
    /// <param name="role">The role associated with the profile.</param>
    /// <param name="key">The metadata key entry configuration to be removed.</param>
    /// <param name="executedByUserId">The unique identifier of the user performing this action.</param>
    /// <returns>A result indicating success (Done) or an error if the profile or key metadata entry is not found.</returns>
    public ResultOf<Done> RemoveProfileMetadata(Role role, string key, Guid executedByUserId)
    {
        var profile = _profiles.FirstOrDefault(p => p.Role == role);
        if (profile is null) return UserErrors.ProfileNotFound;

        var audit = AuditLog.Create(
            originalState: profile,
            action: AuditActionType.Update,
            editedEntityName: NameOfUserProfile,
            customEntityId: $"{this.Id}_{role.NormalizedLowerCaseName}"
        );

        if (!profile.RemoveMetadata(key))
        {
            return UserErrors.PermissionDoesNotExist;
        }

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        profile.MarkUpdated();
        this.MarkUpdated();

        return Done.Default;
    }

    #endregion

    #region Authentication & Password

    /// <summary>
    /// Validates whether the incoming text password matches the stored password hash.
    /// </summary>
    public ResultOf<bool> IsCorrectPasswordHash(string password, IPasswordHasher passwordHasher)
    {
        return passwordHasher.IsCorrectPassword(password, _passwordHash);
    }

    /// <summary>
    /// Securely changes the user's password hash using the provided domain hashing service.
    /// </summary>
    public ResultOf<Done> SetNewPassword(string password, IPasswordHasher passwordHasher)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return UserErrors.EmptyPassword;
        }

        var passwordHashResult = passwordHasher.HashPassword(password);
        if (passwordHashResult.IsFailure)
        {
            return passwordHashResult.Errors;
        }

        _passwordHash = passwordHashResult.Value;
        return Done.Default;
    }

    /// <summary>
    /// Confirms the user's email address verification state.
    /// </summary>
    public ResultOf<Done> ConfirmEmail(
        UserToken token,
        string tokenValue,
        Guid executedByUserId,
        ITokenHasher hasher)
    {
        if (token is null || token.TokenType == TokenType.EmailConfirmation)
        {
            return UserErrors.InvalidTokenType;
        }
        if (Email.IsEmailConfirmed)
        {
            return UserErrors.EmailAlreadyConfirmed;
        }
        var audit = AuditLog.Create(
            originalState: this,
            action: AuditActionType.Update,
            editedEntityName: NameOfUser,
            customEntityId: Id.ToString()
        );
        audit.SetCreator(executedByUserId);

        var markUsedResult = token.MarkUsed(tokenValue, hasher);
        if (markUsedResult.IsFailure)
        {
            return markUsedResult.Errors;
        }

        Email = Email with { IsEmailConfirmed = true, ConfirmedAt = DateTime.UtcNow };
        AddIntegrationEvent(new AuditLogedEvent(audit));
        MarkUpdated();
        return Done.Default;
    }


    /// <summary>
    /// Generates a new Domain UserToken instance using the provided token type specification and string hasher.
    /// Publishes a token creation event containing necessary notification dispatch details.
    /// </summary>
    public ResultOf<UserToken> GenerateToken(
        TokenType tokenType,
        ITokenHasher hasher,
        out string plainToken)
    {
        var tokenCreateResult = UserToken.Create(Id, tokenType, hasher, out plainToken);
        if (tokenCreateResult.IsFailure)
        {
            return tokenCreateResult.Errors;
        }

        var tokenEntity = tokenCreateResult.Value;
        var expiresAt = tokenEntity.CreatedAt.Add(tokenType.Expiration);

        AddIntegrationEvent(new UserTokenCreatedEvent(
            UserId: Id,
            UserEmail: Email.Email,
            ShowName: Name.ShowName,
            PlainToken: plainToken,
            TokenType: tokenType,
            ExpiresAt: expiresAt
        ));

        return tokenEntity;
    }

    /// <summary>
    /// Updates the core profile values for the user account entity. Resets email confirmation state if updated.
    /// </summary>
    public ResultOf<Done> UpdateUserData(
        string? showName = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null)
    {
        var audit = AuditLog.Create(
            originalState: this,
            action: AuditActionType.Update,
            editedEntityName: NameOfUser,
            customEntityId: Id.ToString()
        );
        audit.SetCreator(Id);

        Name = new Name(
                showName ?? Name.ShowName,
                firstName ?? Name.FirstName,
                lastName ?? Name.LastName
            );

        if (email is not null && !email.Equals(Email.Email, StringComparison.OrdinalIgnoreCase))
        {
            Email = new ConfirmedEmail(email, IsEmailConfirmed: false, ConfirmedAt: null);
        }

        MarkUpdated();
        AddIntegrationEvent(new AuditLogedEvent(audit));

        return Done.Default;
    }
    /// <summary>
    /// Modifies the Two-Factor Authentication state flag for the identity record.
    /// </summary>
    public ResultOf<Done> ChangeTFAStatus(bool isEnabled, string password, IPasswordHasher passwordHasher, Guid executedByUserId)
    {
        var passwordCheck = IsCorrectPasswordHash(password, passwordHasher);
        if (passwordCheck.IsFailure || !passwordCheck.Value)
        {
            return UserErrors.InvalidPassword;
        }

        if (TFA.IsTFAEnabled == isEnabled)
        {
            return UserErrors.TFAStatusUnchanged;
        }

        var audit = AuditLog.Create(
            originalState: this,
            action: AuditActionType.Update,
            editedEntityName: NameOfUser,
            customEntityId: Id.ToString()

        );

        TFA = TFA with { IsTFAEnabled = isEnabled, TFAEnabledAt = isEnabled ? DateTime.UtcNow : null };

        MarkUpdated();
        AddIntegrationEvent(new AuditLogedEvent(audit));

        return Done.Updated;
    }
    /// <summary>
    /// Spawns an integration event payload representing a request to generate a new TFA token sequence.
    /// </summary>

    #endregion


    #region Account Status Management

    /// <summary>
    /// Deactivates or activates the user account state and triggers an update audit log event.
    /// </summary>
    /// <param name="isActive">The desired activation status state indicator.</param>
    /// <param name="executedByUserId">The unique identifier of the user performing this action.</param>
    /// <returns>A result indicating success (Done).</returns>
    public ResultOf<Done> SetAccountActivationStatus(bool isActive, Guid executedByUserId)
    {
        if (Status.IsActive == isActive)
        {
            return Done.Default;
        }

        var audit = AuditLog.Create(
            originalState: this,
            action: AuditActionType.Update,
            editedEntityName: NameOfUser,
            customEntityId: Id.ToString()
        );

        Status = Status with { IsActive = isActive };

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        MarkUpdated();

        return Done.Default;
    }

    /// <summary>
    /// Toggles the user's ability to post comments on the platform and tracks the exact timestamp.
    /// </summary>
    /// <param name="isDisabled">The desired restriction state indicator for posting comments.</param>
    /// <param name="executedByUserId">The unique identifier of the user performing this action.</param>
    /// <returns>A result indicating success (Done).</returns>
    public ResultOf<Done> SetCommentingStatus(bool isDisabled, Guid executedByUserId)
    {
        if (Status.IsCommentingDisabled == isDisabled)
        {
            return Done.Default;
        }

        var audit = AuditLog.Create(
            originalState: this,
            action: AuditActionType.Update,
            editedEntityName: NameOfUser,
            customEntityId: Id.ToString()
        );

        Status = Status with
        {
            IsCommentingDisabled = isDisabled,
            CommentingDisabledAt = isDisabled ? DateTime.UtcNow : null
        };

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        MarkUpdated();

        return Done.Default;
    }

    /// <summary>
    /// Blocks or unblocks the user account, handling its activation state and tracking the block timestamp.
    /// </summary>
    /// <param name="isBlocked">The desired block state status indicator.</param>
    /// <param name="executedByUserId">The unique identifier of the user performing this action.</param>
    /// <returns>A result indicating success (Done).</returns>
    public ResultOf<Done> SetBlockStatus(bool isBlocked, Guid executedByUserId)
    {
        if (Status.IsBlocked == isBlocked)
        {
            return Done.Default;
        }

        var audit = AuditLog.Create(
            originalState: this,
            action: AuditActionType.Update,
            editedEntityName: NameOfUser,
            customEntityId: Id.ToString()
        );

        Status = Status with
        {
            IsBlocked = isBlocked,
            BlockedAt = isBlocked ? DateTime.UtcNow : null,
            IsActive = !isBlocked
        };

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        MarkUpdated();

        return Done.Default;
    }

    public void MarkEmailFound()
    {
        AddDomainEvent(new UserEmailFoundEvent(this));
    }

    #endregion



}


#region Value Objects

public record ConfirmedEmail(
    [property: EqualityComponent] string Email,
    [property: EqualityComponent] bool IsEmailConfirmed = false,
    DateTime? ConfirmedAt = null
) : ValueObject;
public record TFAStatus(
    [property: EqualityComponent] bool IsTFAEnabled = false,
    DateTime? TFAEnabledAt = null
) : ValueObject;
public record AccountStatus(
    [property: EqualityComponent] bool IsActive = true,
    [property: EqualityComponent] bool IsCommentingDisabled = false,
    DateTime? CommentingDisabledAt = null,
    [property: EqualityComponent] bool IsBlocked = false,
    DateTime? BlockedAt = null
) : ValueObject;
public record Name(
    [property: EqualityComponent] string ShowName,
    [property: EqualityComponent] string FirstName,
    [property: EqualityComponent] string LastName
) : ValueObject;


#endregion
