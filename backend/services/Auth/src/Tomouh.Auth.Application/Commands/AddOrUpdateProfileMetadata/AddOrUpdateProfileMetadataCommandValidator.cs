using FluentValidation;

namespace Tomouh.Auth.Application.Commands.AddOrUpdateProfileMetadata;

public class AddOrUpdateProfileMetadataCommandValidator : AbstractValidator<AddOrUpdateProfileMetadataCommand>
{
    public AddOrUpdateProfileMetadataCommandValidator()
    {
        RuleFor(x => x.Role)
            .NotNull().WithMessage("Role is required.");

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Metadata key is required.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Metadata value is required.");

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Idempotency key is required.");
    }
}
