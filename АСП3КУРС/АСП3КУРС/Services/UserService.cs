using Microsoft.AspNetCore.Identity;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;
using АСП3курс.Repositories;

namespace АСП3курс.Services;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateUserDto dto, CancellationToken ct = default);
    Task<bool> PatchAsync(int id, PatchUserDto dto, CancellationToken ct = default);
    Task<bool> UpdateProfileAsync(int id, UpdateProfileDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public class UserService : IUserService
{
    private readonly IRepository<User> _repository;
    private readonly IMapper _mapper;

    private readonly IPasswordHasher<User> _hasher;

    public UserService(IRepository<User> repository, IMapper mapper, IPasswordHasher<User> hasher)
    {
        _repository = repository;
        _mapper = mapper;
        _hasher = hasher;
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        var query = _repository.Query();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.IdUser)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<UserDto>(items, total);
    }

    public Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.Query()
            .Where(u => u.IdUser == id)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

    public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<User>(dto);
        entity.PasswordHash = _hasher.HashPassword(entity, dto.Password);
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.IdUser, ct))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            return false;
        }

        _mapper.Map(dto, entity);
        if (dto.Password is not null)
            entity.PasswordHash = _hasher.HashPassword(entity, dto.Password);
        await _repository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> PatchAsync(int id, PatchUserDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
        {
            return false;
        }

        _mapper.Map(dto, entity);
        if (dto.Password is not null)
            entity.PasswordHash = _hasher.HashPassword(entity, dto.Password);
        await _repository.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateProfileAsync(int id, UpdateProfileDto dto, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null)
            return false;

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
