namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.ClassDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class ClassService : IClassService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClassService> _logger;

    public ClassService(AppDbContext context, ILogger<ClassService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Class>> GetAllClassesAsync()
    {
        _logger.LogInformation("ClassService: Fetching all classes");
        return await _context.Classes.ToListAsync();
    }

    public async Task<Class?> GetClassByIdAsync(Guid id)
    {
        _logger.LogInformation("ClassService: Fetching class by Id:{ClassId}", id);
        return await _context.Classes.FindAsync(id);
    }



    /// <summary>
    /// This method creates a new class entity based on the provided ClassCreateDto. It initializes a new Class object with the properties from the DTO, adds it to the database context, and saves the changes asynchronously. The created class entity is then returned.
    /// </summary>
    /// <param name="dto">The data transfer object containing the class details.</param>
    /// <returns>The created Class entity.</returns>
    public async Task<Class> CreateClassAsync(ClassCreateDto dto)
    {
        _logger.LogInformation("ClassService: Creating new class {ClassName} {Section}", dto.Name, dto.Section);
        // Create a new Class entity using the data from the ClassCreateDto
        var cls = new Class
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Section = dto.Section,
            AcademicYear = dto.AcademicYear,
            CreatedAt = DateTime.UtcNow
        };

        _context.Classes.Add(cls);
        await _context.SaveChangesAsync();
        _logger.LogInformation("ClassService: Created class Id:{ClassId}", cls.Id);
        return cls;
    }



    /// <summary>
    /// This method updates an existing class entity based on the provided ClassUpdateDto. It retrieves the class by its ID, checks if it exists, and then updates its properties with the values from the DTO if they are not null. Finally, it saves the changes to the database asynchronously.
    /// </summary>
    /// <param name="id">The ID of the class to update.</param>
    /// <param name="dto">The data transfer object containing the updated class details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateClassAsync(Guid id, ClassUpdateDto dto)
    {
        _logger.LogInformation("ClassService: Updating class Id:{ClassId}", id);
        var cls = await _context.Classes.FindAsync(id);
        if (cls == null)
        {
            _logger.LogWarning("ClassService: Class Id:{ClassId} not found for update", id);
            return;
        }

        if (dto.Name != null) cls.Name = dto.Name;
        if (dto.Section != null) cls.Section = dto.Section;
        if (dto.AcademicYear != null) cls.AcademicYear = dto.AcademicYear;

        await _context.SaveChangesAsync();
        _logger.LogInformation("ClassService: Updated class Id:{ClassId}", id);
    }



    /// <summary>
    /// This method deletes a class entity based on the provided ID. It retrieves the class by its ID, checks if it exists, and then removes it from the database context. Finally, it saves the changes to the database asynchronously.
    /// </summary>
    /// <param name="id">The ID of the class to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteClassAsync(Guid id)
    {
        _logger.LogInformation("ClassService: Deleting class Id:{ClassId}", id);
        var cls = await _context.Classes.FindAsync(id);
        if (cls != null)
        {
            _context.Classes.Remove(cls);
            await _context.SaveChangesAsync();
            _logger.LogInformation("ClassService: Deleted class Id:{ClassId}", id);
        }
        else
        {
            _logger.LogWarning("ClassService: Class Id:{ClassId} not found for deletion", id);
        }
    }




    /// <summary>
    /// This method retrieves a paginated list of classes based on the provided filter criteria. It allows filtering by class name, section, and academic year, and supports pagination through page number and page size parameters.
    /// </summary>
    /// <param name="filterDto">The filter criteria for retrieving classes.</param>
    /// <returns>A PagedResultDto containing the filtered class data and pagination information.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<PagedResultDto<ClassResponseDto>> GetClassesAsync(ClassFilterDto filterDto)
    {
        _logger.LogInformation("ClassService: Querying paged classes");
        // Validate the filterDto parameters, No case sensitivity for Name, Section, and AcademicYear filters
        var query = _context.Classes.AsQueryable();
        if (!string.IsNullOrEmpty(filterDto.Name))
            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{filterDto.Name}%"));
        if (!string.IsNullOrEmpty(filterDto.Section))
            query = query.Where(c => EF.Functions.ILike(c.Section, $"%{filterDto.Section}%"));
        if (!string.IsNullOrEmpty(filterDto.AcademicYear))
            query = query.Where(c => EF.Functions.ILike(c.AcademicYear, $"%{filterDto.AcademicYear}%"));

        bool isDesc = filterDto.SortOrder == AssignmentSystem.Api.Models.Enums.SortOrder.Desc;
        string sortBy = filterDto.SortBy?.ToLower().Trim() ?? "name";

        query = sortBy switch
        {
            "section" => isDesc ? query.OrderByDescending(c => c.Section) : query.OrderBy(c => c.Section),
            "academicyear" => isDesc ? query.OrderByDescending(c => c.AcademicYear) : query.OrderBy(c => c.AcademicYear),
            "createdat" => isDesc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => isDesc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
        };

        // Calculate the total count of filtered classes before applying pagination
        var totalCount = await query.CountAsync();

        // Apply pagination to the query
        var classes = await query
            .Skip((filterDto.PageNumber - 1) * filterDto.PageSize)
            .Take(filterDto.PageSize)
            .Select(c => new ClassResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Section = c.Section,
                AcademicYear = c.AcademicYear,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        _logger.LogInformation("ClassService: Found {TotalCount} classes matching filter", totalCount);

        // Return the paginated result
        return new PagedResultDto<ClassResponseDto>
        {
            Items = classes,
            TotalCount = totalCount,
            PageNumber = filterDto.PageNumber,
            PageSize = filterDto.PageSize
        };

    }
}