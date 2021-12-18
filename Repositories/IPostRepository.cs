using KumportAPI.Post;
using System.Collections.Generic;

namespace KumportAPI.Repositories
{
    public interface IPostRepository
    {
        List<PostModel> Posts();

        PostModel Add(PostModel post);
    }
}
