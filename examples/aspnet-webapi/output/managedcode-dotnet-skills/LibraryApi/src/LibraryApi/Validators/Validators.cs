using FluentValidation;
using LibraryApi.DTOs;

namespace LibraryApi.Validators;

public class CreateAuthorRequestValidator : AbstractValidator<CreateAuthorRequest>
{
    public CreateAuthorRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Biography).MaximumLength(2000);
        RuleFor(x => x.Country).MaximumLength(100);
    }
}

public class UpdateAuthorRequestValidator : AbstractValidator<UpdateAuthorRequest>
{
    public UpdateAuthorRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Biography).MaximumLength(2000);
        RuleFor(x => x.Country).MaximumLength(100);
    }
}

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ISBN).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Publisher).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PageCount).GreaterThan(0).When(x => x.PageCount.HasValue);
        RuleFor(x => x.Language).MaximumLength(50);
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(1);
        RuleFor(x => x.AuthorIds).NotEmpty().WithMessage("At least one author is required.");
        RuleFor(x => x.CategoryIds).NotEmpty().WithMessage("At least one category is required.");
    }
}

public class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ISBN).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Publisher).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PageCount).GreaterThan(0).When(x => x.PageCount.HasValue);
        RuleFor(x => x.Language).MaximumLength(50);
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(1);
        RuleFor(x => x.AuthorIds).NotEmpty().WithMessage("At least one author is required.");
        RuleFor(x => x.CategoryIds).NotEmpty().WithMessage("At least one category is required.");
    }
}

public class CreatePatronRequestValidator : AbstractValidator<CreatePatronRequest>
{
    public CreatePatronRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.MembershipType)
            .NotEmpty()
            .Must(BeAValidMembershipType)
            .WithMessage("MembershipType must be Standard, Premium, or Student.");
    }

    private static bool BeAValidMembershipType(string type) =>
        Enum.TryParse<Models.MembershipType>(type, ignoreCase: true, out _);
}

public class UpdatePatronRequestValidator : AbstractValidator<UpdatePatronRequest>
{
    public UpdatePatronRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.MembershipType)
            .NotEmpty()
            .Must(BeAValidMembershipType)
            .WithMessage("MembershipType must be Standard, Premium, or Student.");
    }

    private static bool BeAValidMembershipType(string type) =>
        Enum.TryParse<Models.MembershipType>(type, ignoreCase: true, out _);
}

public class CreateLoanRequestValidator : AbstractValidator<CreateLoanRequest>
{
    public CreateLoanRequestValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
        RuleFor(x => x.PatronId).GreaterThan(0);
    }
}

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
        RuleFor(x => x.PatronId).GreaterThan(0);
    }
}
