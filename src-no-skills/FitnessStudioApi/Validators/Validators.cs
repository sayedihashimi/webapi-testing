using FluentValidation;
using FitnessStudioApi.DTOs;

namespace FitnessStudioApi.Validators;

public class CreateMembershipPlanValidator : AbstractValidator<CreateMembershipPlanDto>
{
    public CreateMembershipPlanValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DurationMonths).InclusiveBetween(1, 24);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.MaxClassBookingsPerWeek).GreaterThanOrEqualTo(-1);
    }
}

public class CreateMemberValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EmergencyContactPhone).NotEmpty();
    }
}

public class UpdateMemberValidator : AbstractValidator<UpdateMemberDto>
{
    public UpdateMemberValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EmergencyContactPhone).NotEmpty();
    }
}

public class CreateMembershipValidator : AbstractValidator<CreateMembershipDto>
{
    public CreateMembershipValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.MembershipPlanId).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
    }
}

public class FreezeMembershipValidator : AbstractValidator<FreezeMembershipDto>
{
    public FreezeMembershipValidator()
    {
        RuleFor(x => x.FreezeDurationDays).InclusiveBetween(7, 30)
            .WithMessage("Freeze duration must be between 7 and 30 days.");
    }
}

public class CreateInstructorValidator : AbstractValidator<CreateInstructorDto>
{
    public CreateInstructorValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.Bio).MaximumLength(1000);
        RuleFor(x => x.HireDate).NotEmpty();
    }
}

public class CreateClassTypeValidator : AbstractValidator<CreateClassTypeDto>
{
    public CreateClassTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DefaultDurationMinutes).InclusiveBetween(30, 120);
        RuleFor(x => x.DefaultCapacity).InclusiveBetween(1, 50);
        RuleFor(x => x.DifficultyLevel).IsInEnum();
    }
}

public class CreateClassScheduleValidator : AbstractValidator<CreateClassScheduleDto>
{
    public CreateClassScheduleValidator()
    {
        RuleFor(x => x.ClassTypeId).GreaterThan(0);
        RuleFor(x => x.InstructorId).GreaterThan(0);
        RuleFor(x => x.StartTime).NotEmpty().GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");
        RuleFor(x => x.Room).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(30, 120).When(x => x.DurationMinutes.HasValue);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 50).When(x => x.Capacity.HasValue);
    }
}

public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.ClassScheduleId).GreaterThan(0);
        RuleFor(x => x.MemberId).GreaterThan(0);
    }
}
