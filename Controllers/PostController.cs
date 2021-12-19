using Kumport.Common.RequestModels;
using KumportAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace KumportAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private IPostRepository _postRepository;

        public PostController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }
        [Authorize]
        [HttpGet]
        [Route("posts")]
        public IActionResult Posts()        
        {
            var token = Request.Headers["Authorization"];
            var requestId = Request.Headers["Request-Id"];
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Post-Posts => Request:{}", JsonConvert.SerializeObject(new { Token = token, RequestId = requestId}));
            return Ok( _postRepository.Posts());
        }
        
        [Authorize]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Add([FromBody] AddPostRequestModel request)
        {

            var token = Request.Headers["Authorization"];
            var requestId = Request.Headers["Request-Id"];
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Post-Add => Request:{}", JsonConvert.SerializeObject(new { Token = token, RequestId = requestId, PostOwner=request.PostOwner }));

            return Ok(await _postRepository.Add(request));
        }


    }
}
