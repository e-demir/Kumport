using Kumport.Common.RequestModels;
using Kumport.Common.ResponseModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KumportAPI.Repositories
{
    public interface IPostRepository
    {
        PostsResponseModel Posts();

        Task<AddPostResponseModel> Add(AddPostRequestModel request);
        UserPostsResponseModel UserPosts(UserPostsRequestModel request);


    }
}
