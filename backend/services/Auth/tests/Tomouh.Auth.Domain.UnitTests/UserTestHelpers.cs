using Moq;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.Interfaces;

namespace Tomouh.Auth.Domain.UnitTests;

public static class UserTestHelpers
{
    public const string DefaultPassword = "SecurePassword123";
    public const string DefaultHashedPassword = "Hashed_SecurePassword123";

    public static Mock<IPasswordHasher> CreatePasswordHasherMock()
    {
        var mock = new Mock<IPasswordHasher>();

        mock.Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns((string password) => string.IsNullOrWhiteSpace(password)
                ? UserErrors.EmptyPassword
                : $"Hashed_{password}");

        mock.Setup(h => h.HashPassword(DefaultPassword))
            .Returns(DefaultHashedPassword);

        return mock;
    }

    public static User CreateDummyUser(IPasswordHasher? hasher = null)
    {
        hasher ??= CreatePasswordHasherMock().Object;

        return User.CreateLocal(
            showName: "Default User",
            firstName: "Default",
            lastName: "User",
            email: "user@domain.com",
            password: DefaultPassword,
            passwordHasher: hasher).Value;
    }
}