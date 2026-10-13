using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;
using АСП3курс.Repositories;

namespace АСП3курс.Services;

public interface ICartItemService
{
    Task<PagedResult<CartItemDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<CartItemDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CartItemDto> CreateAsync(CreateCartItemDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateCartItemDto dto, CancellationToken ct = default);
    Task<bool> PatchAsync(int id, PatchCartItemDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public class CartItemService : ICartItemService
{
    private readonly IRepository<CartItem> _repository;
    private readonly IMapper _mapper;

    public CartItemService(IRepository<CartItem> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CartItemDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        var query = _repository.Query();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(ci => ci.IdCartItem)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<CartItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<CartItemDto>(items, total);
    }

    public Task<CartItemDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.Query()
            .Where(ci => ci.IdCartItem == id)
            .ProjectTo<CartItemDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

    public async Task<CartItemDto> CreateAsync(CreateCartItemDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<CartItem>(dto);
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.IdCartItem, ct))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateCartItemDto dto, CancellationToken ct = default)
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

    public async Task<bool> PatchAsync(int id, PatchCartItemDto dto, CancellationToken ct = default)
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
