using Kumport.Common.RequestModels;
using KumportAPI.Repositories;
using KumportAPI.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            var validator = new AddPostValidator();
            var requestId = Request.Headers["Request-Id"];
            var logger = NLog.LogManager.GetCurrentClassLogger();
            var rep = await validator.ValidateAsync(request);
            if (!rep.IsValid)
            {
                logger.Error("Post-Add =>  Request:{}", JsonConvert.SerializeObject(new { Message = rep.Errors, StatusCode = 500, RequestId = requestId }));
                return StatusCode(StatusCodes.Status500InternalServerError, new { ErrorMessage = rep.Errors });

            }
            var token = Request.Headers["Authorization"];
            
            logger.Info("Post-Add => Request:{}", JsonConvert.SerializeObject(new { Token = token, RequestId = requestId, PostOwner=request.PostOwner }));

            return Ok(await _postRepository.Add(request));
        }


    }
}
