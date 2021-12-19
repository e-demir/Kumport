using Kumport.Common.RequestModels;
using Kumport.Common.ResponseModels;
using KumportAPI.Post;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KumportAPI.Repositories
{
    public class PostRepository : IPostRepository
    {
        private PostDbContext _context;
        public PostRepository(PostDbContext context)
        {
            _context = context;
        }

        public async Task<AddPostResponseModel> Add(AddPostRequestModel request)
        {
            var postModel = new PostModel();
            postModel.PostOwner = request.PostOwner;
            postModel.PostTitle = request.PostTitle;
            postModel.Image = request.Image;
            postModel.CreatedOn = request.CreatedOn;
            await _context.Posts.AddAsync(postModel);
            await _context.SaveChangesAsync();
            return new AddPostResponseModel();
        }

        public PostsResponseModel Posts()
        {
            var posts = _context.Posts.ToList();
            var response = new PostsResponseModel();
            response.Posts = new List<Kumport.Common.Models.PostModel>();
            foreach (var item in posts)
            {
                var post = new Kumport.Common.Models.PostModel();
                post.CreatedOn = item.CreatedOn;
                post.Image = item.Image;
                post.PostTitle = item.PostTitle;
                post.PostOwner = item.PostOwner;
                response.Posts.Add(post);
            }
            return response;
        }

        public UserPostsResponseModel UserPosts(UserPostsRequestModel request)
        {
            UserPostsResponseModel response = new UserPostsResponseModel();
            response.Posts = new List<Kumport.Common.Models.PostModel>();
            var posts = _context.Posts.Where(x => x.PostOwner == request.Username);
            if (posts!=null && posts.Count() > 0)
            {
                foreach (var item in posts)
                {
                    var post = new Kumport.Common.Models.PostModel();
                    post.CreatedOn = item.CreatedOn;
                    post.FileType = item.FileType;
                    post.Image = item.Image;
                    post.PostOwner = item.PostOwner;
                    post.PostTitle = item.PostTitle;
                    response.Posts.Add(post);
                }
            }
            return response;            
        }
    }
}
