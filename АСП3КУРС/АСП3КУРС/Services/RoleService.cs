using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;
using АСП3курс.Repositories;

namespace АСП3курс.Services;

public interface IRoleService
{
    Task<PagedResult<RoleDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<RoleDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<RoleDto> CreateAsync(CreateRoleDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateRoleDto dto, CancellationToken ct = default);
    Task<bool> PatchAsync(int id, PatchRoleDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public class RoleService : IRoleService
{
    private readonly IRepository<Role> _repository;
    private readonly IMapper _mapper;

    public RoleService(IRepository<Role> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<RoleDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        var query = _repository.Query();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(r => r.IdRole)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<RoleDto>(items, total);
    }

    public Task<RoleDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.Query()
            .Where(r => r.IdRole == id)
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<Role>(dto);
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.IdRole, ct))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateRoleDto dto, CancellationToken ct = default)
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

    public async Task<bool> PatchAsync(int id, PatchRoleDto dto, CancellationToken ct = default)
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
