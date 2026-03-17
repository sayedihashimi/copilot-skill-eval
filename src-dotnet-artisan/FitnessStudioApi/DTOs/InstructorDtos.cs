namespace FitnessStudioApi.DTOs;

public sealed record InstructorResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    DateOnly HireDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateInstructorRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    DateOnly HireDate);

public sealed record UpdateInstructorRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    bool IsActive);
