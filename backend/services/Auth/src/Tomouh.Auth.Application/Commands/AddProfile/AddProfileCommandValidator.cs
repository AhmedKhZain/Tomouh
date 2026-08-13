using FluentValidation;

namespace Tomouh.Auth.Application.Commands.AddProfile;

public class AddProfileCommandValidator : AbstractValidator<AddProfileCommand>
{
    public AddProfileCommandValidator()
    {
        RuleFor(x => x.Role)
            .NotNull().WithMessage("Role is required.");

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Idempotency key is required.");
    }
}
