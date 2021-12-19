using Kumport.Common.RequestModels;
using Kumport.Common.ResponseModels;
using KumportAPI.Authentication;
using KumportAPI.Logging;
using KumportAPI.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            _configuration = configuration;            
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {            
            var requestId = Request.Headers["Request-Id"];
            var logger = NLog.LogManager.GetCurrentClassLogger();
            var validator = new LoginValidator();
            var reply = await validator.ValidateAsync(request);
            if (!reply.IsValid)
            {
                logger.Error("Auth-Login =>  Request:{}", JsonConvert.SerializeObject(new { Message = reply.Errors, StatusCode = 500, RequestId = requestId, Email = request.Email, Password = HashPassword(request.Password) }));
                return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = reply.Errors });
            }
            logger.Info("Auth-Login => Request:{}", JsonConvert.SerializeObject(new {Email = request.Email, Password = HashPassword(request.Password), Message="",RequestId = requestId }));
            LoginResponseModel response = new LoginResponseModel();

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                var ret = await userManager.CheckPasswordAsync(user, request.Password);

                if (!ret)
                {
                    response.IsSuccessful = false;                    
                    response.ReturnMessage = "Invalid password";
                    logger.Error("Auth-Login => Request:{}", JsonConvert.SerializeObject(new { Email = user.Email, Password = user.PasswordHash, Message= "Invalid password",StatusCode=500, RequestId = requestId }));
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
                logger.Info("Auth-Login => Response:{}", JsonConvert.SerializeObject(new {Token=response.Token,TokenExpire=response.TokenExpire,Username=response.Username,Email=user.Email,Message="",StatusCode=200, RequestId = requestId }));
                return Ok(response);
            }
            else
            {
                response.IsSuccessful = false;
                response.ReturnMessage = "User not found";
                logger.Error("Auth-Login => Response:{}", JsonConvert.SerializeObject(new { Email = request.Email, Password = HashPassword(request.Password),Message= "User not found", StatusCode=500,RequestId=requestId }));
                return StatusCode(StatusCodes.Status500InternalServerError,response);
            }

        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel request)
        {
            var requestId = Request.Headers["Request-Id"];
            var logger = NLog.LogManager.GetCurrentClassLogger();
            var validator = new RegisterValidator();
            var response = new RegisterResponseModel();
            var reply = await validator.ValidateAsync(request);
            if (!reply.IsValid)
            {
                logger.Error("Auth-Register =>  Request:{}", JsonConvert.SerializeObject(new { Message = reply.Errors, StatusCode = 500, RequestId = requestId, Email = request.Email, Username = request.Username, Password = HashPassword(request.Password) }));
                return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = reply.Errors });
            }
            logger.Info("Auth-Register => Request:{}", JsonConvert.SerializeObject(new { Email = request.Email, Password = HashPassword(request.Password), Username = request.Username,Message="", RequestId = requestId }));
            var userExists = await userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
            {
                logger.Error("Auth-Register =>  Request:{}", JsonConvert.SerializeObject(new { Email = request.Email, Password = HashPassword(request.Password), Username = request.Username,Message = "User already registered with this E-mail",StatusCode=500,RequestId = requestId }));
                response.ReturnMessage = "User already registered with this E-mail";
                return StatusCode(StatusCodes.Status500InternalServerError, response);
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
                logger.Error("Auth-Register => Response:{}", JsonConvert.SerializeObject(new { Email = request.Email, Password = HashPassword(request.Password), Username = request.Username, Message = errors, StatusCode = 500,RequestId=requestId }));
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            else
            {
                response.ReturnMessage = string.Empty;
                response.IsSuccesful = true;
                logger.Info("Auth-Register => Response:{}", JsonConvert.SerializeObject(new { Email = request.Email, Password = HashPassword(request.Password), Username = request.Username, Message = "", StatusCode = 200, RequestId = requestId }));
                return Ok(response);

            }            
        }        
        private byte[] HashPassword(string password)
        {
            var sha1Hash = new SHA1CryptoServiceProvider();
            var hashValue =  sha1Hash.ComputeHash(Encoding.ASCII.GetBytes(password));
            return hashValue;
        }
    }

}
