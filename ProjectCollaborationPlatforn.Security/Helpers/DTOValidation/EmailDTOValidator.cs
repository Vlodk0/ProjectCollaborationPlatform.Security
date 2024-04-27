using FluentValidation;
using ProjectCollaborationPlatforn.Security.DTOs;

namespace ProjectCollaborationPlatforn.Security.Helpers.DTOValidation
{
    public class EmailDTOValidator : AbstractValidator<EmailDTO>
    {
        public EmailDTOValidator()
        {
            RuleFor(e => e.To)
                .NotEmpty()
                .WithMessage("Email receiver is required");
        }
    }
}
