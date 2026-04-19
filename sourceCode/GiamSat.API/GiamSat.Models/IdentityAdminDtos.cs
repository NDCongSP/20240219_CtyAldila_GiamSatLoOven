using System;
using System.Collections.Generic;

namespace GiamSat.Models
{
    public class IdentityRoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserCount { get; set; }
        public List<string> Claims { get; set; } = new();
    }

    public class IdentityUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Claims { get; set; } = new();
    }

    public class ClaimGroupDto
    {
        public string RoleName { get; set; } = string.Empty;
        public List<string> Claims { get; set; } = new();
    }
}
