using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;
using АСП3курс.Repositories;

namespace АСП3курс.Services;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken ct = default);
    Task<bool> PatchAsync(int id, PatchCategoryDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _repository;
    private readonly IMapper _mapper;

    public CategoryService(IRepository<Category> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CategoryDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        var query = _repository.Query();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.IdCategory)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<CategoryDto>(items, total);
    }

    public Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.Query()
            .Where(c => c.IdCategory == id)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<Category>(dto);
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.IdCategory, ct))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            return false;
        }

        _mapper.Map(dto, entity);
        await _repository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> PatchAsync(int id, PatchCategoryDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            return false;
        }

        _mapper.Map(dto, entity);
        await _repository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            return false;
        }

        _repository.Remove(entity);
        await _repository.SaveChangesAsync(ct);
        return true;
    }
}
