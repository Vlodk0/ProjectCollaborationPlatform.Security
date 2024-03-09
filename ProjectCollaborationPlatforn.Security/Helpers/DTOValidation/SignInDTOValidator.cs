using FluentValidation;
using ProjectCollaborationPlatforn.Security.DTOs;

namespace ProjectCollaborationPlatforn.Security.Helpers.DTOValidation
{
    public class SignInDTOValidator : AbstractValidator<SignInDTO>
    {
        public SignInDTOValidator()
        {
            RuleFor(s => s.Email)
                .NotEmpty()
                .WithMessage("Email address is required")
                .EmailAddress();

            RuleFor(s => s.Password)
                .NotEmpty()
                .WithMessage("Password is required");
        }
    }
}
