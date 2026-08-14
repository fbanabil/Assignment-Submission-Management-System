using System;
using AssignmentSystem.Api.Models.Enums;

namespace Backend.DTOs.SubmissionDTOs;

public class SubmissionFilterDto
{
    public Guid? AssignmentId { get; set; }
    public string? ClassName { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public string? AssignmentTitle { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
    public string? Status { get; set; }
    public string? SortBy { get; set; }
    public SortOrder SortOrder { get; set; } = SortOrder.Desc;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
