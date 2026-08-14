namespace Backend.DTOs.UserDTOs;

using AssignmentSystem.Api.Models.Enums;

public class UserFilterDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? RollNo { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public SortOrder SortOrder { get; set; } = SortOrder.Asc;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
