using FluentValidation;
using ProjectCollaborationPlatforn.Security.DTOs;

namespace ProjectCollaborationPlatforn.Security.Helpers.DTOValidation
{
    public class SignUpDTOValidator : AbstractValidator<SignUpDTO>
    {
        public SignUpDTOValidator()
        {
            RuleFor(s => s.Email)
                .NotEmpty()
                .WithMessage("Email address is required")
                .EmailAddress();

            RuleFor(s => s.Password)
                .NotEmpty()
                .WithMessage("Password is required");

            RuleFor(s => s.Name)
                .NotEmpty()
                .WithMessage("Name is required");

            RuleFor(s => s.Name)
                .Must(s => s.Length > 1)
                .WithMessage("At least 2 characters in name");

            RuleFor(s => s.RoleName)
                .NotEmpty()
                .WithMessage("Role name is required");
        }
    }
}
