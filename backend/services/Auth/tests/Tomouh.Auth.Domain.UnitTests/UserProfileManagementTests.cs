using FluentAssertions;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.Events;
using Tomouh.Auth.Domain.ValueObjects;

namespace Tomouh.Auth.Domain.UnitTests;

public class UserProfileManagementTests
{
    private readonly Guid _executorId = Guid.NewGuid();

    #region Add Profile Tests

    [Fact]
    public void AddProfile_WhenProfileDoesNotExist_ShouldAddProfileAndRaiseAuditEvent()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();

        // Act
        var result = user.AddProfile(Role.SystemAdmin, _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        user.Profiles.Should().HaveCount(2); // Role.User (Default) + Role.SystemAdmin
        user.Profiles.Should().Contain(p => p.Role == Role.SystemAdmin);

        user.IntegrationEvents.OfType<AuditLogedEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void AddProfile_WhenProfileAlreadyExists_ShouldReturnError()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();

        // Act (Role.User is assigned in constructor by default)
        var result = user.AddProfile(Role.User, _executorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(UserErrors.ProfileAlreadyExists);
    }

    #endregion

    #region Permissions Management Tests

    [Fact]
    public void GrantPermissionToProfile_WhenProfileExists_ShouldAddPermission()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();
        const string permission = "Users.Read";

        // Act
        var result = user.GrantPermissionToProfile(Role.User, permission, _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        var userProfile = user.Profiles.First(p => p.Role == Role.User);
        userProfile.Permissions.Should().Contain(permission);
    }

    [Fact]
    public void GrantPermissionToProfile_WhenProfileDoesNotExist_ShouldReturnError()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();

        // Act
        var result = user.GrantPermissionToProfile(Role.SystemAdmin, "Users.Read", _executorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(UserErrors.ProfileNotFound);
    }

    [Fact]
    public void RevokePermissionFromProfile_WhenPermissionExists_ShouldRemovePermission()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();
        const string permission = "Users.Write";
        user.GrantPermissionToProfile(Role.User, permission, _executorId);

        // Act
        var result = user.RevokePermissionFromProfile(Role.User, permission, _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        var userProfile = user.Profiles.First(p => p.Role == Role.User);
        userProfile.Permissions.Should().NotContain(permission);
    }

    #endregion

    #region Metadata Management Tests

    [Fact]
    public void AddOrUpdateProfileMetadata_ShouldAddOrUpdateKeyAndRaiseAuditEvent()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();
        const string key = "Department";
        const string value = "Engineering";

        // Act
        var result = user.AddOrUpdateProfileMetadata(Role.User, key, value, AccountMetadataType.String, true, _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        var userProfile = user.Profiles.First(p => p.Role == Role.User);
        userProfile.Metadata.Should().Contain(m => m.Key == key);
        userProfile.Metadata.First(m => m.Key == key).Value.Should().Be(value);
    }

    [Fact]
    public void RemoveProfileMetadata_WhenKeyExists_ShouldRemoveKey()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();
        const string key = "Department";
        user.AddOrUpdateProfileMetadata(Role.User, key, "Engineering", AccountMetadataType.String, true, _executorId);

        // Act
        var result = user.RemoveProfileMetadata(Role.User, key, _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        var userProfile = user.Profiles.First(p => p.Role == Role.User);
        userProfile.Metadata.Should().NotContain(m => m.Key == key);
    }

    #endregion
}