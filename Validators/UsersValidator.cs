using FluentValidation;
using Kumport.Common.RequestModels;

namespace KumportAPI.Validators
{
    public class UsersValidator : AbstractValidator<UserInfoRequestModel>
    {
       public UsersValidator()
        {
            RuleFor(c => c.Username).NotEmpty().WithMessage("Username can not be empty");
        }
    }
}
