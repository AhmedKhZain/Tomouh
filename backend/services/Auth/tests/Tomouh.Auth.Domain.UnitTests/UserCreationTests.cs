using FluentAssertions;
using Moq;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.Events;
using Tomouh.Auth.Domain.Interfaces;

namespace Tomouh.Auth.Domain.UnitTests;

public class UserCreationTests
{
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    public UserCreationTests()
    {
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _passwordHasherMock
            .Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns((string password) => string.IsNullOrWhiteSpace(password)
                ? UserErrors.EmptyPassword
                : $"hashed_{password}");
    }

    #region Section 1: Local Registration (CreateLocal)

    [Fact]
    public void CreateLocal_WithValidData_ShouldReturnSuccessAndInitializeUserCorrectly()
    {
        // Arrange
        const string showName = "Ahmed Zain";
        const string firstName = "Ahmed";
        const string lastName = "Zain";
        const string email = "ahmed@example.com";
        const string password = "SecurePassword123!";

        // Act
        var result = User.CreateLocal(
            showName: showName,
            firstName: firstName,
            lastName: lastName,
            email: email,
            password: password,
            passwordHasher: _passwordHasherMock.Object);

        // Assert
        result.IsDone.Should().BeTrue();

        var user = result.Value;
        user.Should().NotBeNull();
        user.Name.ShowName.Should().Be(showName);
        user.Name.FirstName.Should().Be(firstName);
        user.Name.LastName.Should().Be(lastName);
        user.MainEmail.Email.Should().Be(email);
        user.MainEmail.IsEmailConfirmed.Should().BeFalse();

        // Check initial state
        user.Status.IsActive.Should().BeTrue();
        user.Status.IsBlocked.Should().BeFalse();
        user.Status.IsCommentingDisabled.Should().BeFalse();

        // Check default role assignment
        user.Profiles.Should().ContainSingle(p => p.Role == Role.User);

        // Check domain event
        user.IntegrationEvents.Should().ContainSingle(e => e is AuditLogedEvent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateLocal_WithEmptyOrNullPassword_ShouldReturnFailure(string? invalidPassword)
    {
        // Act
        var result = User.CreateLocal(
            showName: "Ahmed Zain",
            firstName: "Ahmed",
            lastName: "Zain",
            email: "ahmed@example.com",
            password: invalidPassword!,
            passwordHasher: _passwordHasherMock.Object);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == UserErrors.EmptyPassword.Code);
    }

    [Fact]
    public void CreateLocal_WhenHasherFails_ShouldPropagateHasherErrors()
    {
        // Arrange
        const string password = "WeakPassword";
        var customHasherMock = new Mock<IPasswordHasher>();
        customHasherMock
            .Setup(h => h.HashPassword(password))
            .Returns(UserErrors.EmptyPassword);

        // Act
        var result = User.CreateLocal(
            showName: "Ahmed Zain",
            firstName: "Ahmed",
            lastName: "Zain",
            email: "ahmed@example.com",
            password: password,
            passwordHasher: customHasherMock.Object);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == UserErrors.EmptyPassword.Code);
    }

    #endregion

    #region Section 2: External OAuth Registration (CreateFromExternalProvider)

    [Fact]
    public void CreateFromExternalProvider_WithValidData_ShouldReturnConfirmedUserAndAddExternalLogin()
    {
        // Arrange
        const string showName = "Ahmed Zain";
        const string firstName = "Ahmed";
        const string lastName = "Zain";
        const string email = "ahmed@google.com";
        const string provider = "Google";
        const string providerSubjectId = "google-sub-123456";

        // Act
        var user = User.CreateFromExternalProvider(
            showName: showName,
            firstName: firstName,
            lastName: lastName,
            email: email,
            provider: provider,
            subjectId: providerSubjectId);

        // Assert

        user.MainEmail.IsEmailConfirmed.Should().BeTrue();
        user.ExternalLogins.Should().ContainSingle(l =>
            l.Provider == provider && l.SubjectId == providerSubjectId);

        user.Profiles.Should().ContainSingle(p => p.Role == Role.User);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFromExternalProvider_WhenShowNameNotProvided_ShouldFallbackToFirstNameAndLastName(string? emptyShowName)
    {
        // Arrange
        const string firstName = "Ahmed";
        const string lastName = "Zain";

        // Act
        var result = User.CreateFromExternalProvider(
            showName: emptyShowName,
            firstName: firstName,
            lastName: lastName,
            email: "ahmed@google.com",
            provider: "Google",
            subjectId: "google-sub-123456");

        // Assert
        result.Name.ShowName.Should().Be($"{firstName} {lastName}");
    }

    [Fact]
    public void CreateFromExternalProvider_WhenShowNameProvided_ShouldUseProvidedShowName()
    {
        // Arrange
        const string customShowName = "Dev. Ahmed";

        // Act
        var result = User.CreateFromExternalProvider(
            showName: customShowName,
            firstName: "Ahmed",
            lastName: "Zain",
            email: "ahmed@google.com",
            provider: "Google",
            subjectId: "google-sub-123456");

        // Assert

        result.ShowName.Should().Be(customShowName);
    }

    #endregion

    #region Section 3: Domain Calculated Properties (Computed Properties)

    [Fact]
    public void FullName_ShouldCombineFirstNameAndLastNameTrimmed()
    {
        // Act
        var result = User.CreateFromExternalProvider(
            showName: "Ahmed",
            firstName: "  Ahmed ",
            lastName: " Zain  ",
            email: "ahmed@example.com",
            provider: "Google",
            subjectId: "123");

        // Assert
        result.FullName.Should().Be("Ahmed Zain");
    }

    [Fact]
    public void CanRemoveLastLogin_WhenPasswordIsSetAndHasExternalLogins_ShouldReturnTrue()
    {
        // Arrange
        var user = User.CreateLocal(
            showName: "Ahmed Zain",
            firstName: "Ahmed",
            lastName: "Zain",
            email: "ahmed@example.com",
            password: "Password123!",
            passwordHasher: _passwordHasherMock.Object).Value;

        user.AddExternalLogin("Google", "sub-123");

        // Act & Assert
        user.CanRemoveLastLogin.Should().BeTrue();
    }

    [Fact]
    public void CanRemoveLastLogin_WhenNoPasswordAndOnlyOneExternalLogin_ShouldReturnFalse()
    {
        // Arrange
        var user = User.CreateFromExternalProvider(
            showName: "Ahmed Zain",
            firstName: "Ahmed",
            lastName: "Zain",
            email: "ahmed@google.com",
            provider: "Google",
            subjectId: "sub-123");

        // Act & Assert
        user.CanRemoveLastLogin.Should().BeFalse();
    }

    #endregion
}