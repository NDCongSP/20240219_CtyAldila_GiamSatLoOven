using Microsoft.AspNetCore.Authorization;

namespace GiamSat.Models
{
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
    }
}
