using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;
using АСП3курс.Repositories;

namespace АСП3курс.Services;

public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateOrderDto dto, CancellationToken ct = default);
    Task<bool> PatchAsync(int id, PatchOrderDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _repository;
    private readonly IMapper _mapper;

    public OrderService(IRepository<Order> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<OrderDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        var query = _repository.Query();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(o => o.IdOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<OrderDto>(items, total);
    }

    public Task<OrderDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.Query()
            .Where(o => o.IdOrder == id)
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken ct = default)
    {
        var entity = _mapper.Map<Order>(dto);
        entity.OrderDate = DateTime.UtcNow;

        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);

        return (await GetByIdAsync(entity.IdOrder, ct))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateOrderDto dto, CancellationToken ct = default)
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

    public async Task<bool> PatchAsync(int id, PatchOrderDto dto, CancellationToken ct = default)
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
