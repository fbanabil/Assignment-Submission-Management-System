namespace Backend.DTOs.UserDTOs;

using AssignmentSystem.Api.Models.Enums;

public class UserUpdateDto
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? PhoneNumber { get; set; }
}