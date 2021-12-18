using Kumport.Common.RequestModels;
using Kumport.Common.ResponseModels;
using KumportAPI.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KumportAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager,  IConfiguration configuration)
        {
            this.userManager = userManager;
            _configuration = configuration;
            
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            LoginResponseModel response = new LoginResponseModel();
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                var ret = await userManager.CheckPasswordAsync(user, request.Password);

                if (!ret)
                {
                    response.IsSuccessful = false;
                    response.ReturnMessage = "Invalid password";
                    return StatusCode(StatusCodes.Status500InternalServerError, response);
                }

                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                var signer = new ApplicationUser()
                {
                    Email = user.Email,
                    PasswordHash = user.PasswordHash
                };
                
                response.Token = new JwtSecurityTokenHandler().WriteToken(token);
                response.TokenExpire = token.ValidTo.ToString();
                response.ReturnMessage = string.Empty;
                response.Username = user.UserName;
                response.IsSuccessful = true;
                return Ok(response);
            }
            else
            {
                response.IsSuccessful = false;
                response.ReturnMessage = "User not found";
                return StatusCode(StatusCodes.Status500InternalServerError,response);
            }

        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel request)
        {
            var response = new RegisterResponseModel();
            var userExists = await userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                response.ReturnMessage = "User already registered with this E-mail";
                return BadRequest(response);
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = request.Username
            };
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var item in result.Errors)
                {
                    errors = errors + item.Description;
                }
                response.ReturnMessage = errors;
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            else
            {
                response.ReturnMessage = string.Empty;
                response.IsSuccesful = true;
                return Ok(response);

            }            
        }
    }

}
