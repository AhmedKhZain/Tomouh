using FluentValidation;

namespace Tomouh.Auth.Application.Commands.SetBlockStatus;

public class SetBlockStatusCommandValidator : AbstractValidator<SetBlockStatusCommand>
{
    public SetBlockStatusCommandValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user id is required.");

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Idempotency key is required.");
    }
}
