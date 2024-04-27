using FluentValidation;
using ProjectCollaborationPlatforn.Security.DTOs;

namespace ProjectCollaborationPlatforn.Security.Helpers.DTOValidation
{
    public class ResetCodeDTOValidator : AbstractValidator<ResetCodeDTO>
    {
        public ResetCodeDTOValidator()
        {
            RuleFor(rc => rc.ResetCode)
                .NotEmpty()
                .WithMessage("Reset code is required");

            RuleFor(rc => rc.Email)
                .NotEmpty()
                .WithMessage("Emaild is required")
                .EmailAddress();
        }
    }
}
