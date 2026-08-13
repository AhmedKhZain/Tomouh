using FluentValidation;

namespace Tomouh.Auth.Application.Commands.RevokePermissionFromProfile;

public class RevokePermissionFromProfileCommandValidator : AbstractValidator<RevokePermissionFromProfileCommand>
{
    public RevokePermissionFromProfileCommandValidator()
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
