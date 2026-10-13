using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;
using АСП3курс.Repositories;

namespace АСП3курс.Services;

public interface ICartService
{
    Task<PagedResult<CartDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<CartDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CartDto> CreateAsync(CreateCartDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateCartDto dto, CancellationToken ct = default);
    Task<bool> PatchAsync(int id, PatchCartDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public class CartService : ICartService
{
    private readonly IRepository<Cart> _repository;
    private readonly IMapper _mapper;

    public CartService(IRepository<Cart> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CartDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        var query = _repository.Query();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.IdCart)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<CartDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<CartDto>(items, total);
    }

    public Task<CartDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.Query()
            .Where(c => c.IdCart == id)
            .ProjectTo<CartDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

    public async Task<CartDto> CreateAsync(CreateCartDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<Cart>(dto);
        entity.CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow);

        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);

        return (await GetByIdAsync(entity.IdCart, ct))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateCartDto dto, CancellationToken ct = default)
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

    public async Task<bool> PatchAsync(int id, PatchCartDto dto, CancellationToken ct = default)
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
