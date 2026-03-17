namespace FitnessStudioApi.DTOs;

public sealed record MemberResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string EmergencyContactName,
    string EmergencyContactPhone,
    DateOnly JoinDate,
    bool IsActive,
    ActiveMembershipInfo? ActiveMembership,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ActiveMembershipInfo(
    int MembershipId,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);

public sealed record MemberListResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    bool IsActive,
    DateOnly JoinDate);

public sealed record CreateMemberRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string EmergencyContactName,
    string EmergencyContactPhone);

public sealed record UpdateMemberRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string EmergencyContactName,
    string EmergencyContactPhone);
