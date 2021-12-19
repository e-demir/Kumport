using Kumport.Common.RequestModels;
using Kumport.Common.ResponseModels;
using KumportAPI.Authentication;
using KumportAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace KumportAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private IPostRepository _postRepository;      
        public UserController(UserManager<ApplicationUser> userManager, IPostRepository postRepository)
        {
            this.userManager = userManager;
            _postRepository = postRepository;
        }
        [Authorize]
        [HttpPost]
        [Route("info")]
        public async Task<IActionResult> UserInfo([FromBody] UserInfoRequestModel request)
        {
            var serviceResponse = new UserInfoResponseModel();
            serviceResponse.Posts = new System.Collections.Generic.List<Kumport.Common.Models.PostModel>();
            var requestId = Request.Headers["Request-Id"];
            var logger = NLog.LogManager.GetCurrentClassLogger();            
            logger.Info("Auth-UserInfo => Request:{}", JsonConvert.SerializeObject(new { Username = request.Username, RequestId = requestId }));
            var user = await userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                logger.Error("Auth-Register =>  Response:{}", JsonConvert.SerializeObject(new { Message = "User info was not found", StatusCode = 500, RequestId = requestId }));
                serviceResponse.ReturnMessage = "User info was not found";
                return StatusCode(StatusCodes.Status500InternalServerError, serviceResponse);
            }
            else
            {
                serviceResponse.Username = user.UserName;
                serviceResponse.Email = user.Email;

                var userPosts = _postRepository.UserPosts(new UserPostsRequestModel() { Username = user.UserName });
                if (userPosts.Posts != null && userPosts.Posts.Count > 0)
                {
                    serviceResponse.Posts = userPosts.Posts;
                }

                serviceResponse.IsSuccesfull = true;
                logger.Error("Auth-UserInfo =>  Response:{}", JsonConvert.SerializeObject(new { Message = "", Username = serviceResponse.Username, Email = serviceResponse.Email, StatusCode = 200, RequestId = requestId }));
                return Ok(serviceResponse);
            }
        }
    }
}
