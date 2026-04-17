using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated == true && 
               context.User.HasClaim(PermissionNames.Prefix, requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
