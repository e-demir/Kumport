using KumportAPI.Post;
using System.Collections.Generic;
using System.Linq;

namespace KumportAPI.Repositories
{
    public class PostRepository : IPostRepository
    {
        private PostDbContext _context;
        public PostRepository(PostDbContext context)
        {
            _context = context;
        }

        public PostModel Add(PostModel request)
        {

            _context.Posts.AddAsync(request);
            _context.SaveChangesAsync();           

            return new PostModel();
        }

        public List<PostModel> Posts()
        {
            var posts = _context.Posts.ToList();

            return posts;

            


        }
    }
}
