using FluentValidation;

namespace Tomouh.Auth.Application.Commands.SetCommentingStatus;

public class SetCommentingStatusCommandValidator : AbstractValidator<SetCommentingStatusCommand>
{
    public SetCommentingStatusCommandValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user id is required.");

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Idempotency key is required.");
    }
}
