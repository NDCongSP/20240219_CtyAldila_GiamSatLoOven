using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email,user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                }

                //test add claim
                authClaims.Add(new Claim("testabc", "10000_test"));
                authClaims.Add(new Claim("emailTest", user.Email));

                var token = GetToken(authClaims);

                return Ok(new LoginResult()
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    RefreshToken=Guid.NewGuid().ToString()

                    //Log lai thông tin token để phục vụ cho việc refresh token
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("refreshtoken")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResult))]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            #region Check lại thông tin củ refreshToken xem có đúng ko thì mới cho refresh

            #endregion

            var claim = JwtHelper.GetClaimsPrincipalFromJwt(model.OldToken);

            var user = await _userManager.FindByNameAsync(claim.Identity.Name);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                }

                //test add claim
                authClaims.Add(new Claim("testabc", "10000_test"));

                var token = GetToken(authClaims);

                return Ok(new LoginResult()
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    RefreshToken=Guid.NewGuid().ToString()

                    //update lai thông tin token để phục vụ cho việc refresh token
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Response))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(Response))]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "User already exists!" });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
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
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

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
            var checkPass = await _userManager.CheckPasswordAsync(userExists, model.OldPassword);

            if (userExists == null)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "User not found!" });

            if (checkPass == false)
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "Wrong old pass!" });

            if (model.NewPassword != model.ReNewPassword)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = "New password does not match." });
            }

            var result = await _userManager.ChangePasswordAsync(userExists, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Error", Message = $"{result.Errors}." });
            }

            //update kieu ko can nhap pass cu
            //_userManager.RemovePasswordAsync(userExists);
            //_userManager.AddPasswordAsync(userExists, "password moi");

            //if (!result.Succeeded)
            //    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "User update successfully!" });
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
                userModels.Add(new UserModel()
                {
                   Id=item.Id,
                   UserName=item.UserName,  
                   Email=item.Email,
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
            var user=await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status200OK, new Response() { Status = "Error", Message = "User not found!" });
            }

            var res = _userManager.DeleteAsync(user);

            return Ok(new Response() { Status="Success",Message="Delete user success."});
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(60),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
