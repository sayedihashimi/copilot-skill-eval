using FluentValidation;
using LibraryApi.DTOs;

namespace LibraryApi.Validators;

public class CreateAuthorDtoValidator : AbstractValidator<CreateAuthorDto>
{
    public CreateAuthorDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Biography).MaximumLength(2000);
        RuleFor(x => x.Country).MaximumLength(100);
    }
}

public class UpdateAuthorDtoValidator : AbstractValidator<UpdateAuthorDto>
{
    public UpdateAuthorDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Biography).MaximumLength(2000);
        RuleFor(x => x.Country).MaximumLength(100);
    }
}

public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class CreateBookDtoValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ISBN).NotEmpty().MaximumLength(20)
            .Matches(@"^(?:\d{9}[\dXx]|\d{13})$").WithMessage("ISBN must be a valid 10 or 13 digit ISBN.");
        RuleFor(x => x.Publisher).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PageCount).GreaterThan(0).When(x => x.PageCount.HasValue);
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Language).MaximumLength(50);
        RuleFor(x => x.AuthorIds).NotEmpty().WithMessage("At least one author is required.");
    }
}

public class UpdateBookDtoValidator : AbstractValidator<UpdateBookDto>
{
    public UpdateBookDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ISBN).NotEmpty().MaximumLength(20)
            .Matches(@"^(?:\d{9}[\dXx]|\d{13})$").WithMessage("ISBN must be a valid 10 or 13 digit ISBN.");
        RuleFor(x => x.Publisher).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PageCount).GreaterThan(0).When(x => x.PageCount.HasValue);
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Language).MaximumLength(50);
        RuleFor(x => x.AuthorIds).NotEmpty().WithMessage("At least one author is required.");
    }
}

public class CreatePatronDtoValidator : AbstractValidator<CreatePatronDto>
{
    public CreatePatronDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.MembershipType).IsInEnum();
    }
}

public class UpdatePatronDtoValidator : AbstractValidator<UpdatePatronDto>
{
    public UpdatePatronDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.MembershipType).IsInEnum();
    }
}

public class CreateLoanDtoValidator : AbstractValidator<CreateLoanDto>
{
    public CreateLoanDtoValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
        RuleFor(x => x.PatronId).GreaterThan(0);
    }
}

public class CreateReservationDtoValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationDtoValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
        RuleFor(x => x.PatronId).GreaterThan(0);
    }
}
