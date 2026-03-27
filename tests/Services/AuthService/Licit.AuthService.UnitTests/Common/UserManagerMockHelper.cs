using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Common;

public static class UserManagerMockHelper
{
    public static UserManager<ApplicationUser> CreateMock()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);
    }
}
