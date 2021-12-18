using System.Collections.Generic;

namespace KumportAPI.Post
{
    public class PostsResponseModel
    {
        public List<PostModel> Posts { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
