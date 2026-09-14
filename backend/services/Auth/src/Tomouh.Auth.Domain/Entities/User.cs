using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.Events;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Auth.Domain.ValueObjects;
using Tomouh.Shared.Kernel.AuditLogs;
using Tomouh.Shared.Kernel.BaseTypes;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Domain.Entities;

public class User : AuditableAggregateRoot<Guid>
{
    public static readonly string NameOfUser = typeof(User).Name;
    public static readonly string NameOfUserProfile = $"{NameOfUser}:{typeof(UserProfile).Name}";

    public Name Name { get; private set; } = null!;
    public TFAStatus TFA { get; private set; } = null!;
    public EmailStatus MainEmail { get; private set; } = null!;
    public AccountStatus Status { get; private set; } = null!;

    // Computed property combining FirstName and LastName dynamically
    public string FullName => $"{Name.FirstName} {Name.LastName}";
    public string ShowName => Name.ShowName;


    private readonly List<ExternalLogin> _externalLogins = new();
    public IReadOnlyCollection<ExternalLogin> ExternalLogins => _externalLogins.AsReadOnly();


    private string? _passwordHash;
    public string? ProfilePhotoPath { get; private set; }

    private readonly List<UserProfile> _profiles = new();
    public IReadOnlyCollection<UserProfile> Profiles => _profiles.AsReadOnly();

    private readonly HashSet<AccountMetadata> _metadata = new();
    public IReadOnlyCollection<AccountMetadata> Metadata => _metadata;

    // Private Constructor للـ Standard Registration
    // Private Constructor للـ Standard Registration
    private User(
        string showName,
        string firstName,
        string lastName,
        string email,
        string? profilePhotoPath = null,
        ExternalLogin? initialExternalLogin = null)
        : base(Guid.NewGuid(), null)
    {
        Name = new Name(showName, firstName, lastName);

        bool isConfirmed = initialExternalLogin is not null;
        MainEmail = new EmailStatus(email, isEmailConfirmed: isConfirmed, confirmedAt: isConfirmed ? DateTime.UtcNow : null);

        TFA = new TFAStatus();
        Status = new AccountStatus();
        ProfilePhotoPath = profilePhotoPath;
        _profiles = new List<UserProfile>();
        _externalLogins = new List<ExternalLogin>();

        if (initialExternalLogin is not null)
        {
            _externalLogins.Add(initialExternalLogin);
        }

        AddProfile(Role.User, Id);
    }

    public static ResultOf<User> CreateLocal(
        string showName,
        string firstName,
        string lastName,
        string email,
        string password,
        IPasswordHasher passwordHasher)
    {
        var user = new User(showName, firstName, lastName, email);
        var passwordResult = user.SetNewPassword(password, passwordHasher);

        if (passwordResult.IsFailure)
        {
            return passwordResult.Errors;
        }

        return user;
    }

    /// <summary>
    /// Creates a user account initialized from an external OAuth provider payload.
    /// </summary>
    public static User CreateFromExternalProvider(
        string provider,
        string subjectId,
        string email,
        string firstName,
        string lastName,
        string? profilePhotoPath = null,
        string? showName = null)
    {
        var trimmedFirstName = firstName?.Trim() ?? string.Empty;
        var trimmedLastName = lastName?.Trim() ?? string.Empty;
        var effectiveShowName = string.IsNullOrWhiteSpace(showName)
            ? $"{trimmedFirstName} {trimmedLastName}"
            : showName;

        var externalLogin = ExternalLogin.Create(provider, subjectId);

        return new User(
            showName: effectiveShowName,
            firstName: trimmedFirstName,
            lastName: trimmedLastName,
            email: email,
            profilePhotoPath: profilePhotoPath,
            initialExternalLogin: externalLogin
        );
    }
    public bool CanRemoveLastLogin => !string.IsNullOrWhiteSpace(_passwordHash) || _externalLogins.Count > 1;

    private User() : base() { }

    #region External Logins Management

    /// <summary>
    /// Links a new external OAuth provider login to this user account.
    /// </summary>
    public ResultOf<Done> AddExternalLogin(string provider, string subjectId)
    {
        if (_externalLogins.Any(l => l.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase)))
        {
            return UserErrors.ExternalLoginAlreadyExists;
        }

        _externalLogins.Add(ExternalLogin.Create(provider, subjectId));
        MarkUpdated();

        return Done.Default;
    }

    /// <summary>
    /// Removes an existing external OAuth provider login from this user account.
    /// </summary>
    public ResultOf<Done> RemoveExternalLogin(string provider)
    {
        var existing = _externalLogins.FirstOrDefault(l => l.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            return UserErrors.ExternalLoginNotFound;
        }
        if (!CanRemoveLastLogin)
        {
            return UserErrors.CannotRemoveLastLoginMethod;
        }

        _externalLogins.Remove(existing);
        MarkUpdated();

        return Done.Default;
    }

    #endregion

    #region Json & MongoDB Deserialization Constructor

    [BsonConstructor]
    [JsonConstructor]
    private User(
            Guid id,
            Name name,
            EmailStatus email,
            TFAStatus tfa,
            AccountStatus status,
            string? passwordHash,
            List<UserProfile> profiles,
            List<ExternalLogin>? externalLogins,
            string? profilePhotoPath,
            HashSet<AccountMetadata>? metadata,
            DateTime? lastUpdate,
            DateTime createdAt,
            Guid? createdBy) : base(id, null)
    {
        Name = name;
        MainEmail = email;
        TFA = tfa;
        Status = status;
        _passwordHash = passwordHash;
        ProfilePhotoPath = profilePhotoPath;
        _profiles = profiles ?? new List<UserProfile>();
        _externalLogins = externalLogins ?? new List<ExternalLogin>();
        _metadata = metadata ?? new HashSet<AccountMetadata>();
        CreatedAt = createdAt;
        LastUpdate = lastUpdate;
        CreatedBy = createdBy;
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

    #region General Metadata Management

    /// <summary>
    /// Adds or updates a general metadata key-value pair on the user and triggers an update audit log event.
    /// </summary>
    /// <param name="key">The metadata key configuration entry.</param>
    /// <param name="value">The metadata value payload linked to the entry key.</param>
    /// <param name="executedByUserId">The unique identifier of the user performing this action.</param>
    /// <returns>A result indicating success (Done) or an error if audit logging fails.</returns>
    public ResultOf<Done> AddOrUpdateMetadata(string key, string value, Guid executedByUserId)
    {
        var audit = AuditLog.Create(
            originalState: this,
            action: AuditActionType.Update,
            editedEntityName: NameOfUser,
            customEntityId: Id.ToString()
        );

        var existing = _metadata.FirstOrDefault(m => m.Key == key);

        if (existing is not null)
        {
            var updated = existing.WithValue(value);
            _metadata.Remove(existing);
            _metadata.Add(updated);
        }
        else
        {
            _metadata.Add(new AccountMetadata(key, value, isPublic: true));
        }

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        MarkUpdated();

        return Done.Updated;
    }

    /// <summary>
    /// Removes a general metadata key from the user and triggers an update audit log event.
    /// </summary>
    /// <param name="key">The metadata key entry configuration to be removed.</param>
    /// <param name="executedByUserId">The unique identifier of the user performing this action.</param>
    /// <returns>A result indicating success (Done) or an error if the key metadata entry is not found.</returns>
    public ResultOf<Done> RemoveMetadata(string key, Guid executedByUserId)
    {
        var existing = _metadata.FirstOrDefault(m => m.Key == key);

        if (existing is null)
        {
            return UserErrors.MetadataNotFound;
        }

        var audit = AuditLog.Create(
            originalState: this,
            action: AuditActionType.Update,
            editedEntityName: NameOfUser,
            customEntityId: Id.ToString()
        );

        _metadata.Remove(existing);

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        MarkUpdated();

        return Done.Updated;
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
        ITokenHasher hasher)
    {
        if (token is null || token.TokenType == TokenType.EmailConfirmation)
        {
            return UserErrors.InvalidTokenType;
        }
        if (MainEmail.IsEmailConfirmed)
        {
            return UserErrors.EmailAlreadyConfirmed;
        }
        var audit = AuditLog.Create(
            originalState: this,
            action: AuditActionType.Update,
            editedEntityName: NameOfUser,
            customEntityId: Id.ToString()
        );
        audit.SetCreator(Id);

        var markUsedResult = token.MarkUsed(tokenValue, hasher);
        if (markUsedResult.IsFailure)
        {
            return markUsedResult.Errors;
        }

        MainEmail = new EmailStatus(MainEmail.Email, true, DateTime.UtcNow);
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
            UserEmail: MainEmail.Email,
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
                showName?.Trim() ?? Name.ShowName,
                firstName?.Trim() ?? Name.FirstName,
                lastName?.Trim() ?? Name.LastName
            );

        if (email is not null && !email.Equals(MainEmail.Email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            MainEmail = new EmailStatus(email, isEmailConfirmed: false, confirmedAt: null);
        }

        MarkUpdated();
        AddIntegrationEvent(new AuditLogedEvent(audit));

        return Done.Default;
    }
    /// <summary>
    /// Modifies the Two-Factor Authentication state flag for the identity record.
    /// </summary>
    public ResultOf<Done> ChangeTFAStatus(bool isEnabled, string password, IPasswordHasher passwordHasher)
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
            customEntityId: Id.ToString(),
            createdBy: Id
        );

        TFA = new TFAStatus(isEnabled, isEnabled ? DateTime.UtcNow : null);

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

        Status = new AccountStatus(
            isActive,
            Status.IsCommentingDisabled,
            Status.CommentingDisabledAt,
            Status.IsBlocked,
            Status.BlockedAt);

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

        Status = new AccountStatus(
            Status.IsActive,
            isDisabled,
            isDisabled ? DateTime.UtcNow : null,
            Status.IsBlocked,
            Status.BlockedAt);

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
    public ResultOf<Done> SetBlockStatus(bool isBlocked, Guid? executedByUserId = null)
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

        Status = new AccountStatus(
            !isBlocked,
            Status.IsCommentingDisabled,
            Status.CommentingDisabledAt,
            isBlocked,
            isBlocked ? DateTime.UtcNow : null);

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        MarkUpdated();

        return Done.Default;
    }
    public ResultOf<Done> SetActivationStatus(bool isActive, Guid executedByUserId)
    {
        if (Status.IsActive == isActive)
        {
            return Done.Default;
        }

        if (Status.IsBlocked)
            return UserErrors.CannotActivateBlockedAccount;

        var audit = AuditLog.Create(
            originalState: this,
            action: AuditActionType.Update,
            editedEntityName: NameOfUser,
            customEntityId: Id.ToString()
        );

        Status = new AccountStatus(
            isActive,
            Status.IsCommentingDisabled,
            Status.CommentingDisabledAt,
            false,
            null);

        audit.SetCreator(executedByUserId);
        AddIntegrationEvent(new AuditLogedEvent(audit));
        MarkUpdated();

        return Done.Default;
    }


    #endregion
    public void MarkEmailFound()
    {
        AddDomainEvent(new UserEmailFoundEvent(this));
    }


}
