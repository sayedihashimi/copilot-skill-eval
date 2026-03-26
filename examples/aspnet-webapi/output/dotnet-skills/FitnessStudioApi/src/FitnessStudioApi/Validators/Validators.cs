using FluentValidation;
using FitnessStudioApi.DTOs.MembershipPlan;
using FitnessStudioApi.DTOs.Member;
using FitnessStudioApi.DTOs.Membership;
using FitnessStudioApi.DTOs.Instructor;
using FitnessStudioApi.DTOs.ClassType;
using FitnessStudioApi.DTOs.ClassSchedule;
using FitnessStudioApi.DTOs.Booking;

namespace FitnessStudioApi.Validators;

public sealed class CreateMembershipPlanValidator : AbstractValidator<CreateMembershipPlanDto>
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

public sealed class UpdateMembershipPlanValidator : AbstractValidator<UpdateMembershipPlanDto>
{
    public UpdateMembershipPlanValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DurationMonths).InclusiveBetween(1, 24);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.MaxClassBookingsPerWeek).GreaterThanOrEqualTo(-1);
    }
}

public sealed class CreateMemberValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.DateOfBirth).Must(dob =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - dob.Year;
            if (dob.AddYears(age) > today) age--;
            return age >= 16;
        }).WithMessage("Member must be at least 16 years old.");
        RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EmergencyContactPhone).NotEmpty();
    }
}

public sealed class UpdateMemberValidator : AbstractValidator<UpdateMemberDto>
{
    public UpdateMemberValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EmergencyContactPhone).NotEmpty();
    }
}

public sealed class CreateMembershipValidator : AbstractValidator<CreateMembershipDto>
{
    public CreateMembershipValidator()
    {
        RuleFor(x => x.MemberId).GreaterThan(0);
        RuleFor(x => x.MembershipPlanId).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
    }
}

public sealed class FreezeMembershipValidator : AbstractValidator<FreezeMembershipDto>
{
    public FreezeMembershipValidator()
    {
        RuleFor(x => x.FreezeDurationDays).InclusiveBetween(7, 30)
            .WithMessage("Freeze duration must be between 7 and 30 days.");
    }
}

public sealed class CreateInstructorValidator : AbstractValidator<CreateInstructorDto>
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

public sealed class UpdateInstructorValidator : AbstractValidator<UpdateInstructorDto>
{
    public UpdateInstructorValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.Bio).MaximumLength(1000);
    }
}

public sealed class CreateClassTypeValidator : AbstractValidator<CreateClassTypeDto>
{
    public CreateClassTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DefaultDurationMinutes).InclusiveBetween(30, 120);
        RuleFor(x => x.DefaultCapacity).InclusiveBetween(1, 50);
        RuleFor(x => x.DifficultyLevel).Must(d =>
            Enum.TryParse<Models.Enums.DifficultyLevel>(d, true, out _))
            .WithMessage("Invalid difficulty level. Must be Beginner, Intermediate, Advanced, or AllLevels.");
    }
}

public sealed class UpdateClassTypeValidator : AbstractValidator<UpdateClassTypeDto>
{
    public UpdateClassTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DefaultDurationMinutes).InclusiveBetween(30, 120);
        RuleFor(x => x.DefaultCapacity).InclusiveBetween(1, 50);
        RuleFor(x => x.DifficultyLevel).Must(d =>
            Enum.TryParse<Models.Enums.DifficultyLevel>(d, true, out _))
            .WithMessage("Invalid difficulty level. Must be Beginner, Intermediate, Advanced, or AllLevels.");
    }
}

public sealed class CreateClassScheduleValidator : AbstractValidator<CreateClassScheduleDto>
{
    public CreateClassScheduleValidator()
    {
        RuleFor(x => x.ClassTypeId).GreaterThan(0);
        RuleFor(x => x.InstructorId).GreaterThan(0);
        RuleFor(x => x.StartTime).GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");
        RuleFor(x => x.Room).NotEmpty().MaximumLength(50);
    }
}

public sealed class CreateBookingValidator : AbstractValidator<CreateBookingDto>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.ClassScheduleId).GreaterThan(0);
        RuleFor(x => x.MemberId).GreaterThan(0);
    }
}
