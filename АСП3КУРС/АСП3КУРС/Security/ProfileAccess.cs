using Microsoft.AspNetCore.Authorization;
using АСП3курс.DTOs;

namespace АСП3курс.Security;

public static class Access
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Profile = "OwnProfile";
    public const string LoginLimit = "LoginLimit";
}

public sealed class ProfileOwnerRequirement : IAuthorizationRequirement { }

public sealed class ProfileOwnerHandler : AuthorizationHandler<ProfileOwnerRequirement, UserDto>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ProfileOwnerRequirement requirement, UserDto resource)
    {
        if (context.User.IsInRole(Access.Admin) ||
            (context.User.IsInRole(Access.User) &&
             int.TryParse(context.User.FindFirst("sub")?.Value, out var id) && id == resource.IdUser))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
