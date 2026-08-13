namespace Backend.DTOs.UserDTOs;

using AssignmentSystem.Api.Models.Enums;

public class UserCreateResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
