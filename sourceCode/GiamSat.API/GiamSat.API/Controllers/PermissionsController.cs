using GiamSat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GiamSat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionsController(
            ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<IdentityRoleDto>>> GetRoles()
        {
            var roles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();

            var userRoleCounts = await _dbContext.UserRoles
                .GroupBy(x => x.RoleId)
                .Select(x => new { RoleId = x.Key, Count = x.Count() })
                .ToDictionaryAsync(x => x.RoleId, x => x.Count);

            var rolePermissions = await _dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(x => x.Permission != null && x.Permission.IsActive)
                .GroupBy(x => x.RoleId)
                .Select(x => new
                {
                    RoleId = x.Key,
                    Claims = x.Select(y => y.Permission!.Code).Distinct().ToList()
                })
                .ToDictionaryAsync(x => x.RoleId, x => x.Claims);

            var result = roles.Select(r => new IdentityRoleDto
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                Description = null,
                UserCount = userRoleCounts.TryGetValue(r.Id, out var c) ? c : 0,
                Claims = rolePermissions.TryGetValue(r.Id, out var claims) ? claims : new List<string>()
            }).ToList();

            return Ok(result);
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] IdentityRoleDto model)
        {
            if (string.IsNullOrWhiteSpace(model?.Name))
                return BadRequest(new { Message = "Role name is required." });

            if (await _roleManager.RoleExistsAsync(model.Name))
                return BadRequest(new { Message = "Role already exists." });

            var result = await _roleManager.CreateAsync(new IdentityRole(model.Name));
            if (!result.Succeeded)
                return BadRequest(new { Message = string.Join("; ", result.Errors.Select(x => x.Description)) });

            return Ok();
        }

        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] IdentityRoleDto model)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model?.Name))
                return BadRequest(new { Message = "Role name is required." });

            role.Name = model.Name;
            role.NormalizedName = model.Name.ToUpperInvariant();

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
                return BadRequest(new { Message = string.Join("; ", result.Errors.Select(x => x.Description)) });

            return Ok();
        }

        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (string.Equals(role.Name, UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Message = "Cannot delete Admin role." });

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                return BadRequest(new { Message = string.Join("; ", result.Errors.Select(x => x.Description)) });

            return Ok();
        }

        [HttpGet("permissions")]
        public async Task<ActionResult<IEnumerable<SecurityPermission>>> GetPermissions()
        {
            var permissions = await _dbContext.SecurityPermissions
                .Where(x => x.IsActive)
                .OrderBy(x => x.Module)
                .ThenBy(x => x.Action)
                .ToListAsync();

            return Ok(permissions);
        }

        [HttpPut("roles/{roleId}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] List<string> permissionCodes)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return NotFound();

            permissionCodes ??= new List<string>();
            permissionCodes = permissionCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

            var currentRolePermissions = await _dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(x => x.RoleId == roleId)
                .ToListAsync();

            var permissionsToKeep = currentRolePermissions
                .Where(rp => rp.Permission != null && permissionCodes.Contains(rp.Permission.Code))
                .ToList();

            var permissionsToRemove = currentRolePermissions
                .Except(permissionsToKeep)
                .ToList();

            _dbContext.RolePermissions.RemoveRange(permissionsToRemove);

            var existingCodes = permissionsToKeep.Select(p => p.Permission!.Code).ToList();
            var newCodes = permissionCodes.Except(existingCodes).ToList();

            var permissionsToAdd = await _dbContext.SecurityPermissions
                .Where(p => newCodes.Contains(p.Code) && p.IsActive)
                .ToListAsync();

            foreach (var perm in permissionsToAdd)
            {
                _dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = perm.Id
                });
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<IdentityUserDto>>> GetUsers()
        {
            var users = await _userManager.Users.OrderBy(x => x.UserName).ToListAsync();

            var result = new List<IdentityUserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var claims = await _dbContext.RolePermissions
                    .Include(rp => rp.Role)
                    .Include(rp => rp.Permission)
                    .Where(x => x.Role != null && roles.Contains(x.Role.Name) && x.Permission != null && x.Permission.IsActive)
                    .Select(x => x.Permission!.Code)
                    .Distinct()
                    .ToListAsync();

                result.Add(new IdentityUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    Claims = claims
                });
            }

            return Ok(result);
        }

        [HttpPut("users/{id}/roles")]
        public async Task<IActionResult> UpdateUserRoles(string id, [FromBody] List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (roles != null && roles.Count > 0)
            {
                var validRoles = roles.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
                await _userManager.AddToRolesAsync(user, validRoles);
            }

            return Ok();
        }

        [HttpGet("claims")]
        public async Task<ActionResult<IEnumerable<ClaimGroupDto>>> GetRoleClaims()
        {
            var roles = await _roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
            var rolePermissions = await _dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(x => x.Permission != null && x.Permission.IsActive)
                .ToListAsync();

            var result = new List<ClaimGroupDto>();

            foreach (var role in roles)
            {
                var claims = rolePermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.Permission!.Code)
                    .Distinct()
                    .ToList();

                result.Add(new ClaimGroupDto
                {
                    RoleName = role.Name ?? string.Empty,
                    Claims = claims
                });
            }

            return Ok(result);
        }

        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            await PermissionSeeder.SeedAsync(_dbContext, _roleManager);
            return Ok(new { Status = "Success", Message = "Permissions seeded." });
        }
    }
}
