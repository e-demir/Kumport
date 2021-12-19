using FluentValidation;
using Kumport.Common.RequestModels;

namespace KumportAPI.Validators
{
    public class AddPostValidator : AbstractValidator<AddPostRequestModel>
    {
        public AddPostValidator()
        {
            RuleFor(c => c.CreatedOn).NotEmpty().NotNull().WithMessage("Creation date can not be null or empty");
            RuleFor(c => c.FileType).NotEmpty().WithMessage("File type can not be null or empty");
            RuleFor(c => c.Image).NotNull().WithMessage("Imagecan not be null or empty");
            RuleFor(c => c.PostOwner).NotEmpty().WithMessage("Post Owner can not be null or empty");
            RuleFor(c => c.PostTitle).NotEmpty().WithMessage("Post Title can not be null or empty");
        }
    }
}
