using FluentAssertions;
using Tomouh.Auth.Domain.Events;

namespace Tomouh.Auth.Domain.UnitTests;

public class UserAccountStatusTests
{
    private readonly Guid _executorId = Guid.NewGuid();

    #region Block Status Tests

    [Fact]
    public void SetBlockStatus_ToTrue_ShouldBlockUserDeactivateAccountAndRaiseAuditEvent()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();
        var initialEventsCount = user.IntegrationEvents.Count;

        // Act
        var result = user.SetBlockStatus(isBlocked: true, executedByUserId: _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        user.Status.IsBlocked.Should().BeTrue();
        user.Status.IsActive.Should().BeFalse();
        user.Status.BlockedAt.Should().NotBeNull();

        user.IntegrationEvents.Count.Should().Be(initialEventsCount + 1);
        user.IntegrationEvents.Should().Contain(e => e is AuditLogedEvent);
    }

    [Fact]
    public void SetBlockStatus_ToFalse_WhenUserWasBlocked_ShouldUnblockUser()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();
        user.SetBlockStatus(isBlocked: true, executedByUserId: _executorId);

        // Act
        var result = user.SetBlockStatus(isBlocked: false, executedByUserId: _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        user.Status.IsBlocked.Should().BeFalse();
        user.Status.BlockedAt.Should().BeNull();
    }

    [Fact]
    public void SetBlockStatus_WhenStatusIsSame_ShouldNotRaiseNewAuditEvent()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();
        user.SetBlockStatus(isBlocked: true, executedByUserId: _executorId);
        var eventsCountAfterFirstBlock = user.IntegrationEvents.Count;

        // Act
        var result = user.SetBlockStatus(isBlocked: true, executedByUserId: _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        user.IntegrationEvents.Count.Should().Be(eventsCountAfterFirstBlock);
    }

    #endregion

    #region Activation Status Tests

    [Fact]
    public void SetActivationStatus_ToFalse_ShouldDeactivateAccount()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();

        // Act
        var result = user.SetActivationStatus(isActive: false, executedByUserId: _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        user.Status.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SetActivationStatus_ToTrue_WhenUserIsBlocked_ShouldReturnError()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();
        user.SetBlockStatus(isBlocked: true, executedByUserId: _executorId);

        // Act
        var result = user.SetActivationStatus(isActive: true, executedByUserId: _executorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(UserErrors.CannotActivateBlockedAccount);
        user.Status.IsActive.Should().BeFalse();
    }

    #endregion

    #region Commenting Status Tests

    [Fact]
    public void SetCommentingStatus_ToDisabled_ShouldDisableCommentingAndSetTimestamp()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();

        // Act
        var result = user.SetCommentingStatus(isDisabled: true, executedByUserId: _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        user.Status.IsCommentingDisabled.Should().BeTrue();
        user.Status.CommentingDisabledAt.Should().NotBeNull();
    }

    [Fact]
    public void SetCommentingStatus_ToEnabled_ShouldEnableCommentingAndResetTimestamp()
    {
        // Arrange
        var user = UserTestHelpers.CreateDummyUser();
        user.SetCommentingStatus(isDisabled: true, executedByUserId: _executorId);

        // Act
        var result = user.SetCommentingStatus(isDisabled: false, executedByUserId: _executorId);

        // Assert
        result.IsDone.Should().BeTrue();
        user.Status.IsCommentingDisabled.Should().BeFalse();
        user.Status.CommentingDisabledAt.Should().BeNull();
    }

    #endregion
}