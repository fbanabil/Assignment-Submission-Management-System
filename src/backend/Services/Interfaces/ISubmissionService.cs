namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;

public interface ISubmissionService
{
    Task<IEnumerable<Submission>> GetAllSubmissionsAsync();
    Task<Submission?> GetSubmissionByIdAsync(Guid id);
    Task<Submission> CreateSubmissionAsync(SubmissionCreateDto dto);
    Task UpdateSubmissionAsync(Guid id, SubmissionUpdateDto dto);
    Task GradeSubmissionAsync(GradeDto dto, Guid graderId);
}