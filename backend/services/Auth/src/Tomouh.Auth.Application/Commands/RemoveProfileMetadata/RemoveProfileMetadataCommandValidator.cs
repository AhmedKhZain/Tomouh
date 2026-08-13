using FluentValidation;

namespace Tomouh.Auth.Application.Commands.RemoveProfileMetadata;

public class RemoveProfileMetadataCommandValidator : AbstractValidator<RemoveProfileMetadataCommand>
{
    public RemoveProfileMetadataCommandValidator()
    {
        RuleFor(x => x.Role)
            .NotNull().WithMessage("Role is required.");

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Metadata key is required.");

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Idempotency key is required.");
    }
}
