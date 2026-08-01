using Common.ResultOf.Errors;

namespace Tomouh.Domain.Scholarships;

public static class ScholarshipErrors
{
    public static readonly Error PaperRequirementNotFound = Error.NotFound(
        "Scholarship.PaperRequirementNotFound",
        "The requested paper requirement was not found.");

    public static readonly Error QualificationRequirementNotFound = Error.NotFound(
        "Scholarship.QualificationRequirementNotFound",
        "The requested qualification requirement was not found.");
}