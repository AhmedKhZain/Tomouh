using FluentAssertions;
using Tomouh.Auth.Domain.Entities;

namespace Tomouh.Auth.Domain.UnitTests;

public class UserExternalLoginTests
{
    #region Add External Login Tests

    [Fact]
    public void AddExternalLogin_WhenProviderIsNew_ShouldAddProvider()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();

        // Act
        var result = user.AddExternalLogin("GitHub", "github-sub-123");

        // Assert
        result.IsDone.Should().BeTrue();
        user.ExternalLogins.Should().ContainSingle(l =>
            l.Provider == "GitHub" && l.SubjectId == "github-sub-123");
    }

    [Fact]
    public void AddExternalLogin_WhenProviderAlreadyExists_ShouldReturnError()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();
        user.AddExternalLogin("GitHub", "github-sub-123");

        // Act (Case Insensitive Validation Test)
        var result = user.AddExternalLogin("GITHUB", "github-dup-sub");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(UserErrors.ExternalLoginAlreadyExists);
    }

    #endregion

    #region Remove External Login Tests

    [Fact]
    public void RemoveExternalLogin_WhenUserCanRemoveAndProviderExists_ShouldRemoveProvider()
    {
        // Arrange (User with password and one external login)
        var user = UserTestHelpers.CreateDummyUser();
        user.AddExternalLogin("Google", "google-123");

        // Act
        var result = user.RemoveExternalLogin("Google");

        // Assert
        result.IsDone.Should().BeTrue();
        user.ExternalLogins.Should().NotContain(l => l.Provider == "Google");
    }

    [Fact]
    public void RemoveExternalLogin_WhenProviderDoesNotExist_ShouldReturnError()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();

        // Act
        var result = user.RemoveExternalLogin("Facebook");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(UserErrors.ExternalLoginNotFound);
    }

    [Fact]
    public void RemoveExternalLogin_WhenItIsLastLoginMethodAndNoPassword_ShouldReturnError()
    {
        // Arrange (User created from OAuth with NO password)
        var user = User.CreateFromExternalProvider(
            showName: "OAuth User",
            firstName: "OAuth",
            lastName: "User",
            email: "oauth@example.com",
            provider: "Google",
            subjectId: "google-123");

        // Act
        var result = user.RemoveExternalLogin("Google");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(UserErrors.CannotRemoveLastLoginMethod);
        user.ExternalLogins.Should().ContainSingle(l => l.Provider == "Google");
    }

    #endregion
}