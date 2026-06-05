using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResult))]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var permissionCodes = await GetPermissionCodesAsync(userRoles);
                foreach (var permissionCode in permissionCodes)
                {
                    authClaims.Add(new Claim(PermissionNames.Prefix, permissionCode));
                }

                // Permissions are now fully managed via DB — no wildcard bypass

                var token = GetToken(authClaims);

                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);
                if (refreshTokenValidityInMinutes <= 0) refreshTokenValidityInMinutes = 1;

                var refreshToken = Guid.NewGuid().ToString();
                var refreshTokenExpiry = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);

                // Lưu refresh token vào DB
                var refreshTokenEntity = new FT17_RefreshToken
                {
                    Id = Guid.NewGuid(),
                    Token = refreshToken,
                    UserId = user.Id,
                    JwtId = authClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value,
                    IsUsed = false,
                    IsRevoked = false,
                    CreatedAt = DateTime.Now,
                    ExpiryDate = refreshTokenExpiry
                };

                _dbContext.FT17_RefreshTokens.Add(refreshTokenEntity);
                await _dbContext.SaveChangesAsync();

                return Ok(new LoginResult()
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiration = refreshTokenExpiry
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("refreshtoken")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResult))]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            // 1. Validate refresh token từ DB
            var storedToken = await _dbContext.FT17_RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == model.RefreshToken);

            if (storedToken == null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Refresh token không tồn tại." });

            if (storedToken.IsUsed)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Refresh token đã được sử dụng." });

            if (storedToken.IsRevoked)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Refresh token đã bị thu hồi." });

            if (storedToken.ExpiryDate < DateTime.Now)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Refresh token đã hết hạn." });

            // 2. Validate access token (lấy thông tin user)
            var claim = JwtHelper.GetClaimsPrincipalFromJwt(model.OldToken);
            if (claim?.Identity?.Name == null)
                return Unauthorized();

            var user = await _userManager.FindByNameAsync(claim.Identity.Name);
            if (user == null || user.Id != storedToken.UserId)
                return Unauthorized();

            // 3. Đánh dấu refresh token cũ đã sử dụng
            storedToken.IsUsed = true;
            _dbContext.FT17_RefreshTokens.Update(storedToken);

            // 4. Tạo access token mới
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var permissionCodes = await GetPermissionCodesAsync(userRoles);
            foreach (var permissionCode in permissionCodes)
            {
                authClaims.Add(new Claim(PermissionNames.Prefix, permissionCode));
            }

            var token = GetToken(authClaims);

            // 5. Tạo refresh token mới (rotation)
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);
            if (refreshTokenValidityInMinutes <= 0) refreshTokenValidityInMinutes = 1;

            var newRefreshToken = Guid.NewGuid().ToString();
            var newRefreshTokenExpiry = storedToken.ExpiryDate; // Giữ nguyên thời hạn cũ thay vì cộng thêm

            var newRefreshTokenEntity = new FT17_RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = newRefreshToken,
                UserId = user.Id,
                JwtId = authClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value,
                IsUsed = false,
                IsRevoked = false,
                CreatedAt = DateTime.Now,
                ExpiryDate = newRefreshTokenExpiry
            };

            _dbContext.FT17_RefreshTokens.Add(newRefreshTokenEntity);
            await _dbContext.SaveChangesAsync();

            return Ok(new LoginResult()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiration = newRefreshTokenExpiry
            });
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Response))]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "User already exists!" });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            foreach (var item in model.Roles)
            {
                if (await _roleManager.RoleExistsAsync(item))
                {
                    await _userManager.AddToRoleAsync(user, item);
                }
            }
            //if (await _roleManager.RoleExistsAsync(UserRoles.User))
            //{
            //    await _userManager.AddToRoleAsync(user, UserRoles.User);
            //}

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost]
        [Route("register-admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Response))]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            foreach (var roleName in UserRoles.DefaultRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }

            if (await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }


        [HttpPost]
        [Route("updatePass")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Response))]
        public async Task<IActionResult> UpdatePass([FromBody] UpdateModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists == null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "User not found!" });

            var checkPass = await _userManager.CheckPasswordAsync(userExists, model.OldPassword);
            if (!checkPass)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Mật khẩu cũ không chính xác!" });

            if (model.NewPassword != model.ReNewPassword)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Mật khẩu mới không khớp!" });
            }

            var result = await _userManager.ChangePasswordAsync(userExists, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = $"Lỗi cập nhật mật khẩu: {errors}" });
            }

            return Ok(new Response { Status = "Success", Message = "Cập nhật mật khẩu thành công!" });
        }

        [HttpPost]
        [Route("resetpass")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Response))]
        public async Task<IActionResult> ResetPass([FromBody] ResetPasswordModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Người dùng không tồn tại!" });

            // Removing password and adding a new one is a safe way to force a reset in this context
            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, "123@456");

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = $"Lỗi reset mật khẩu: {errors}" });
            }

            return Ok(new Response { Status = "Success", Message = "Reset mật khẩu thành công. Mật khẩu mới là 123@456" });
        }

        [HttpPost]
        [Route("checkuser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Response))]
        public async Task<IActionResult> CheckUser([FromBody] LoginModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            var checkPass = await _userManager.CheckPasswordAsync(userExists, model.Password);

            if (userExists == null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "User not found!" });

            if (!checkPass)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Wrong pass!" });

            return Ok(new Response { Status = "Success", Message = "User OK!" });
        }

        [HttpGet]
        [Route("GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserModel>))]//quy dinh kieu du lieu trả về
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(List<UserModel>))]
        public async Task<IActionResult> GetAllUsers()
        {
            List<UserModel> userModels = new List<UserModel>();

            var u = _userManager.Users.ToList();

            foreach (var item in u)
            {
                var user = await _userManager.FindByNameAsync(item.UserName);
                var roles = await _userManager.GetRolesAsync(user);

                //var uu = new UserModel();
                //uu.Id = item.Id;
                //uu.UserName = item.UserName;
                //uu.Email = item.Email;
                //foreach (var item1 in roles)
                //{
                //    uu.Roles.Add(item1);
                //}

                userModels.Add(new UserModel()
                {
                    Id = item.Id,
                    UserName= item.UserName,
                    Email= item.Email,
                    Roles=roles.ToList(),
                });
            }

            return Ok(userModels);
        }

        [HttpPost]
        [Route("DeleteUser")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Response))]
        public async Task<IActionResult> DeleteUser([FromBody] UserModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status200OK, new Response() { Status = "Error", Message = "User not found!" });
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                return StatusCode(StatusCodes.Status200OK, new Response() { Status = "Error", Message = "Not allowed to delete the admin account" });
            }

            // Đã bỏ check Role. Identity UserManager.DeleteAsync sẽ tự động cascade xóa các record trong bảng AspNetUserRoles.

            // Xóa tất cả refresh token của user trước khi xóa user
            var userTokens = await _dbContext.FT17_RefreshTokens
                .Where(t => t.UserId == user.Id)
                .ToListAsync();
            if (userTokens.Any())
            {
                _dbContext.FT17_RefreshTokens.RemoveRange(userTokens);
                await _dbContext.SaveChangesAsync();
            }

            var res = await _userManager.DeleteAsync(user);

            return Ok(new Response() { Status="Success",Message="Delete user success."});
        }

        private async Task<List<string>> GetPermissionCodesAsync(IEnumerable<string> roles)
        {
            var roleNames = roles.ToList();
            if (!roleNames.Any()) return new List<string>();

            // Retrieve role IDs given role names
            var roleIds = await _roleManager.Roles
                .Where(r => roleNames.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            if (!roleIds.Any()) return new List<string>();

            // Manual mapping because NO navigation property in RoleToPermissions
            var roleMap = await _dbContext.RoleToPermissions
                .Where(x => roleIds.Contains(x.RoleId))
                .Select(x => x.PermissionId)
                .ToListAsync();

            var permIds = roleMap.Distinct().ToList();

            var perms = await _dbContext.Permissions
                .Where(p => permIds.Contains(p.Id) && p.IsActived == true)
                .Select(p => p.Module + "." + p.Actions)
                .Distinct()
                .ToListAsync();

            return perms;
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            _ = int.TryParse(_configuration["JWT:TokenValidityInSeconds"], out int tokenValidityInSeconds);
            if (tokenValidityInSeconds <= 0) tokenValidityInSeconds = 20;

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddSeconds(tokenValidityInSeconds),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
