using FluentValidation;

namespace Tomouh.Auth.Application.Commands.GrantPermissionToProfile;

public class GrantPermissionToProfileCommandValidator : AbstractValidator<GrantPermissionToProfileCommand>
{
    public GrantPermissionToProfileCommandValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user id is required.");

        RuleFor(x => x.Role)
            .NotNull().WithMessage("Role is required.");

        RuleFor(x => x.Permission)
            .NotEmpty().WithMessage("Permission is required.");

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Idempotency key is required.");
    }
}
