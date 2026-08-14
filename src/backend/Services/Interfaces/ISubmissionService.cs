namespace AssignmentSystem.Api.Services.Interfaces;

using AssignmentSystem.Api.Models.Entities;
using Backend.DTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.UserDTOs;
using Backend.DTOs.StudentDTOs;

using Microsoft.AspNetCore.Http;

public interface ISubmissionService
{
    Task<IEnumerable<Submission>> GetAllSubmissionsAsync();
    Task<Submission?> GetSubmissionByIdAsync(Guid id);
    Task<Submission> CreateSubmissionAsync(SubmissionCreateDto dto);
    Task UpdateSubmissionAsync(Guid id, SubmissionUpdateDto dto);
    Task GradeSubmissionAsync(GradeDto dto, Guid graderId);
    Task<SubmissionSummaryDto> GetSubmissionSummaryAsync();
    Task<PagedResultDto<SubmissionResponseDto>> GetSubmissionsAsync(SubmissionFilterDto filterDto);
    Task<int> GetUngradedSubmissionsCount(Guid teacherId);
    Task<StudentSubmissionDetailDto> CreateStudentSubmissionAsync(Guid studentId, StudentSubmissionCreateDto dto);
    Task<FileUploadResponseDto> UploadAssignmentFileAsync(IFormFile file, string webRootPath);
    Task UnsubmitAssignmentAsync(Guid studentId, Guid submissionId);
    Task<PagedResultDto<StudentSubmissionHistoryResponseDto>> GetStudentSubmissionHistoryPagedAsync(Guid studentId, StudentSubmissionHistoryFilterDto filterDto);
}