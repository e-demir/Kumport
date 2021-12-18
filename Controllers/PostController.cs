using KumportAPI.Post;
using KumportAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        [Route("posts")]
        public IActionResult Posts()
        
        {
            return Ok(_postRepository.Posts());
        }
        
        [HttpPost]
        [Route("add")]
        public IActionResult Add(PostModel request)
        {
            return Ok(_postRepository.Add(request));
        }


    }
}
