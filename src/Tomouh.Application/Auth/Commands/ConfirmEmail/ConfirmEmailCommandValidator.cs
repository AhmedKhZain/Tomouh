using FluentValidation;

namespace Tomouh.Application.Auth.Queries.ConfirmEmail;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Confirmation token is required.");

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Idempotency key is required.");
    }
}
