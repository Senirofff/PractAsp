using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using АСП3курс.DTOs;
using АСП3курс.Models;
using АСП3курс.Repositories;
using АСП3курс.Services.Exceptions;

namespace АСП3курс.Services;

public interface IReservationService
{
    Task<PagedResult<ReservationDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ReservationDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Бизнес-операция: зарезервировать товар (уменьшить остаток) и создать запись о событии.</summary>
    Task<ReservationDto> ReserveAsync(CreateReservationDto dto, CancellationToken ct = default);

    /// <summary>Обратная операция: вернуть товар на склад и закрыть активную запись о событии.</summary>
    Task<ReservationDto> ReleaseAsync(int id, CancellationToken ct = default);

    Task<bool> UpdateAsync(int id, UpdateReservationDto dto, CancellationToken ct = default);
    Task<bool> PatchAsync(int id, PatchReservationDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public class ReservationService : IReservationService
{
    private readonly IRepository<Reservation> _reservations;
    private readonly IRepository<Product> _products;
    private readonly IRepository<User> _users;
    private readonly IMapper _mapper;

    public ReservationService(
        IRepository<Reservation> reservations,
        IRepository<Product> products,
        IRepository<User> users,
        IMapper mapper)
    {
        _reservations = reservations;
        _products = products;
        _users = users;
        _mapper = mapper;
    }

    public async Task<PagedResult<ReservationDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        (page, pageSize) = Paging.Normalize(page, pageSize);

        var query = _reservations.Query();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(r => r.IdReservation)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ReservationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<ReservationDto>(items, total);
    }

    public Task<ReservationDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _reservations.Query()
            .Where(r => r.IdReservation == id)
            .ProjectTo<ReservationDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

    public async Task<ReservationDto> ReserveAsync(CreateReservationDto dto, CancellationToken ct = default)
    {
        if (dto.Quantity <= 0)
        {
            throw new BusinessRuleException("Количество для резервирования должно быть больше нуля.");
        }

        var user = await _users.GetByIdAsync(dto.IdUser, ct)
            ?? throw new NotFoundException($"Пользователь с id={dto.IdUser} не найден.");

        if (!user.IsActive)
        {
            throw new BusinessRuleException($"Пользователь '{user.Login}' деактивирован и не может резервировать товары.");
        }

        var product = await _products.GetByIdAsync(dto.IdProduct, ct)
            ?? throw new NotFoundException($"Товар с id={dto.IdProduct} не найден.");

        if (product.StockQuantity < dto.Quantity)
        {
            throw new BusinessRuleException(
                $"Недостаточно товара '{product.Name}' на складе: доступно {product.StockQuantity}, запрошено {dto.Quantity}.");
        }

        // Две связанные сущности меняются в одном запросе:
        // 1) у товара уменьшается остаток; 2) создаётся запись о факте резервирования.
        product.StockQuantity -= dto.Quantity;

        var reservation = _mapper.Map<Reservation>(dto);
        reservation.Status = ReservationStatuses.Active;
        reservation.CreatedAt = DateTime.UtcNow;
        reservation.ReleasedAt = null;

        await _reservations.AddAsync(reservation, ct);

        // Один SaveChangesAsync сохраняет изменения обоих репозиториев,
        // так как они используют один и тот же scoped DbContext.
        await _reservations.SaveChangesAsync(ct);

        return (await GetByIdAsync(reservation.IdReservation, ct))!;
    }

    public async Task<ReservationDto> ReleaseAsync(int id, CancellationToken ct = default)
    {
        var reservation = await _reservations.Query()
            .Include(r => r.IdProductNavigation)
            .FirstOrDefaultAsync(r => r.IdReservation == id, ct);

        if (reservation is null || reservation.Status != ReservationStatuses.Active)
        {
            throw new NotFoundException($"Активное резервирование с id={id} не найдено.");
        }

        // Возврат ресурса на склад и закрытие записи о событии — один SaveChangesAsync.
        reservation.IdProductNavigation.StockQuantity += reservation.Quantity;
        reservation.Status = ReservationStatuses.Released;
        reservation.ReleasedAt = DateTime.UtcNow;

        await _reservations.SaveChangesAsync(ct);

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> UpdateAsync(int id, UpdateReservationDto dto, CancellationToken ct = default)
    {
        var entity = await _reservations.GetByIdAsync(id, ct);
        if (entity is null)
        {
            return false;
        }

        _mapper.Map(dto, entity);
        await _reservations.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> PatchAsync(int id, PatchReservationDto dto, CancellationToken ct = default)
    {
        var entity = await _reservations.GetByIdAsync(id, ct);
        if (entity is null)
        {
            return false;
        }

        _mapper.Map(dto, entity);
        await _reservations.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _reservations.GetByIdAsync(id, ct);
        if (entity is null)
        {
            return false;
        }

        _reservations.Remove(entity);
        await _reservations.SaveChangesAsync(ct);
        return true;
    }
}
