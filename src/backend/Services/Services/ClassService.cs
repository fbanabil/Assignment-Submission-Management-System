namespace AssignmentSystem.Api.Services.Services;

using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.ClassDTOs;
using Backend.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

public class ClassService : IClassService
{
    private readonly AppDbContext _context;

    public ClassService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Class>> GetAllClassesAsync() =>
        await _context.Classes.ToListAsync();

    public async Task<Class?> GetClassByIdAsync(Guid id) =>
        await _context.Classes.FindAsync(id);



    /// <summary>
    /// This method creates a new class entity based on the provided ClassCreateDto. It initializes a new Class object with the properties from the DTO, adds it to the database context, and saves the changes asynchronously. The created class entity is then returned.
    /// </summary>
    /// <param name="dto">The data transfer object containing the class details.</param>
    /// <returns>The created Class entity.</returns>
    public async Task<Class> CreateClassAsync(ClassCreateDto dto)
    {
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
        var cls = await _context.Classes.FindAsync(id);
        if (cls == null) return;

        if (dto.Name != null) cls.Name = dto.Name;
        if (dto.Section != null) cls.Section = dto.Section;
        if (dto.AcademicYear != null) cls.AcademicYear = dto.AcademicYear;

        await _context.SaveChangesAsync();
    }



    /// <summary>
    /// This method deletes a class entity based on the provided ID. It retrieves the class by its ID, checks if it exists, and then removes it from the database context. Finally, it saves the changes to the database asynchronously.
    /// </summary>
    /// <param name="id">The ID of the class to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteClassAsync(Guid id)
    {
        var cls = await _context.Classes.FindAsync(id);
        if (cls != null)
        {
            _context.Classes.Remove(cls);
            await _context.SaveChangesAsync();
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
        // Validate the filterDto parameters, No case sensitivity for Name, Section, and AcademicYear filters
        var query = _context.Classes.AsQueryable();
        if (!string.IsNullOrEmpty(filterDto.Name))
            query = query.Where(c => EF.Functions.ILike(c.Name, $"%{filterDto.Name}%"));
        if (!string.IsNullOrEmpty(filterDto.Section))
            query = query.Where(c => EF.Functions.ILike(c.Section, $"%{filterDto.Section}%"));
        if (!string.IsNullOrEmpty(filterDto.AcademicYear))
            query = query.Where(c => EF.Functions.ILike(c.AcademicYear, $"%{filterDto.AcademicYear}%"));

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