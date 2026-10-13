using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;
using АСП3курс.Repositories;

namespace АСП3курс.Services;

public interface IOrderItemService
{
    Task<PagedResult<OrderItemDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<OrderItemDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<OrderItemDto> CreateAsync(CreateOrderItemDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateOrderItemDto dto, CancellationToken ct = default);
    Task<bool> PatchAsync(int id, PatchOrderItemDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public class OrderItemService : IOrderItemService
{
    private readonly IRepository<OrderItem> _repository;
    private readonly IMapper _mapper;

    public OrderItemService(IRepository<OrderItem> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<OrderItemDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        var query = _repository.Query();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(oi => oi.IdOrderItem)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<OrderItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<OrderItemDto>(items, total);
    }

    public Task<OrderItemDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.Query()
            .Where(oi => oi.IdOrderItem == id)
            .ProjectTo<OrderItemDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

    public async Task<OrderItemDto> CreateAsync(CreateOrderItemDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<OrderItem>(dto);
        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.IdOrderItem, ct))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateOrderItemDto dto, CancellationToken ct = default)
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

    public async Task<bool> PatchAsync(int id, PatchOrderItemDto dto, CancellationToken ct = default)
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
