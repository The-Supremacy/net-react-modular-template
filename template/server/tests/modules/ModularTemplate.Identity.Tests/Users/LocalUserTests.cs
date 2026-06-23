using ModularTemplate.Identity.Users;
using ModularTemplate.Identity.Users.Events;
using Shouldly;

namespace ModularTemplate.Identity.Tests.Users;

public sealed class LocalUserTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Create_WhenIdentityIsValid_RecordsCreatedEvent()
    {
        LocalUser user = LocalUser.Create(" oidc ", " subject-1 ", " Ada ", "ada@example.test");

        user.Provider.ShouldBe("oidc");
        user.Subject.ShouldBe("subject-1");
        user.DisplayName.ShouldBe("Ada");
        user.Email!.Value.ShouldBe("ada@example.test");
        user.PendingDomainEvents.Single().ShouldBeOfType<LocalUserCreatedDomainEvent>();
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(" ada@example.test")]
    [InlineData("ada.example.test")]
    [InlineData("@example.test")]
    [InlineData("ada@")]
    public void Create_WhenEmailIsInvalid_ThrowsArgumentException(string email)
    {
        Should.Throw<InvalidEmailAddressException>(
            () => LocalUser.Create("oidc", "subject-1", "Ada", email));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MarkSeen_WhenUserIsSeen_RecordsSeenEvent()
    {
        LocalUser user = LocalUser.Create("oidc", "subject-1", "Ada", "ada@example.test");
        user.ClearPendingDomainEvents();

        user.MarkSeen(" Ada Lovelace ", "ada.lovelace@example.test");

        user.DisplayName.ShouldBe("Ada Lovelace");
        user.Email!.Value.ShouldBe("ada.lovelace@example.test");
        user.PendingDomainEvents.Single().ShouldBeOfType<LocalUserSeenDomainEvent>();
    }
}
