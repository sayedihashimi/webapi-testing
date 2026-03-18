using FluentValidation;
using LibraryApi.DTOs;

namespace LibraryApi.Validators;

public class CreateAuthorValidator : AbstractValidator<CreateAuthorDto>
{
    public CreateAuthorValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Biography).MaximumLength(2000);
        RuleFor(x => x.Country).MaximumLength(100);
    }
}

public class UpdateAuthorValidator : AbstractValidator<UpdateAuthorDto>
{
    public UpdateAuthorValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Biography).MaximumLength(2000);
        RuleFor(x => x.Country).MaximumLength(100);
    }
}

public class CreateCategoryValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class CreateBookValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ISBN).NotEmpty().MaximumLength(20)
            .Matches(@"^(?:\d{9}[\dXx]|\d{13}|(?:\d{1,5}-){3}\d{1}|(?:\d{1,5}-){4}\d{1})$")
            .WithMessage("ISBN must be a valid ISBN-10 or ISBN-13 format");
        RuleFor(x => x.Publisher).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PageCount).GreaterThan(0).When(x => x.PageCount.HasValue);
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Language).MaximumLength(50);
        RuleFor(x => x.AuthorIds).NotEmpty().WithMessage("At least one author is required");
    }
}

public class UpdateBookValidator : AbstractValidator<UpdateBookDto>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ISBN).NotEmpty().MaximumLength(20)
            .Matches(@"^(?:\d{9}[\dXx]|\d{13}|(?:\d{1,5}-){3}\d{1}|(?:\d{1,5}-){4}\d{1})$")
            .WithMessage("ISBN must be a valid ISBN-10 or ISBN-13 format");
        RuleFor(x => x.Publisher).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.PageCount).GreaterThan(0).When(x => x.PageCount.HasValue);
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Language).MaximumLength(50);
        RuleFor(x => x.AuthorIds).NotEmpty().WithMessage("At least one author is required");
    }
}

public class CreatePatronValidator : AbstractValidator<CreatePatronDto>
{
    public CreatePatronValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.MembershipType).IsInEnum();
    }
}

public class UpdatePatronValidator : AbstractValidator<UpdatePatronDto>
{
    public UpdatePatronValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.MembershipType).IsInEnum();
    }
}

public class CreateLoanValidator : AbstractValidator<CreateLoanDto>
{
    public CreateLoanValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
        RuleFor(x => x.PatronId).GreaterThan(0);
    }
}

public class CreateReservationValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.BookId).GreaterThan(0);
        RuleFor(x => x.PatronId).GreaterThan(0);
    }
}
