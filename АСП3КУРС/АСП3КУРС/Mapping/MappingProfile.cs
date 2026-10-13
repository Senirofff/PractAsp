using AutoMapper;
using АСП3курс.DTOs;
using АСП3курс.Models;

namespace АСП3курс.Mapping;

/// <summary>
/// Единая конфигурация AutoMapper. Ручных методов маппинга в проекте нет:
/// преобразование сущностей в DTO и обратно описано только здесь.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ---------------- Category ----------------
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products.Count));
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();
        CreateMap<PatchCategoryDto, Category>()
            .IgnoreNullMembers();

        // ---------------- Product ----------------
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Category, o => o.MapFrom(s => s.IdCategoryNavigation));
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
        CreateMap<PatchProductDto, Product>()
            .IgnoreNullMembers();

        // ---------------- User ----------------
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.IdRoleNavigation));
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
        CreateMap<PatchUserDto, User>()
            .IgnoreNullMembers();

        // ---------------- Role ----------------
        CreateMap<Role, RoleDto>();
        CreateMap<CreateRoleDto, Role>();
        CreateMap<UpdateRoleDto, Role>();
        CreateMap<PatchRoleDto, Role>()
            .IgnoreNullMembers();

        // ---------------- Cart ----------------
        CreateMap<Cart, CartDto>()
            .ForMember(d => d.User, o => o.MapFrom(s => s.IdUserNavigation))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.CartItems));
        CreateMap<CreateCartDto, Cart>();
        CreateMap<UpdateCartDto, Cart>();
        CreateMap<PatchCartDto, Cart>()
            .IgnoreNullMembers();

        // ---------------- CartItem ----------------
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.Product, o => o.MapFrom(s => s.IdProductNavigation));
        CreateMap<CreateCartItemDto, CartItem>();
        CreateMap<UpdateCartItemDto, CartItem>();
        CreateMap<PatchCartItemDto, CartItem>()
            .IgnoreNullMembers();

        // ---------------- Order ----------------
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.User, o => o.MapFrom(s => s.IdUserNavigation))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.OrderItems));
        CreateMap<CreateOrderDto, Order>();
        CreateMap<UpdateOrderDto, Order>();
        CreateMap<PatchOrderDto, Order>()
            .IgnoreNullMembers();

        // ---------------- OrderItem ----------------
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.Product, o => o.MapFrom(s => s.IdProductNavigation));
        CreateMap<CreateOrderItemDto, OrderItem>();
        CreateMap<UpdateOrderItemDto, OrderItem>();
        CreateMap<PatchOrderItemDto, OrderItem>()
            .IgnoreNullMembers();

        // ---------------- Reservation ----------------
        CreateMap<Reservation, ReservationDto>()
            .ForMember(d => d.User, o => o.MapFrom(s => s.IdUserNavigation))
            .ForMember(d => d.Product, o => o.MapFrom(s => s.IdProductNavigation));
        CreateMap<CreateReservationDto, Reservation>();
        CreateMap<UpdateReservationDto, Reservation>();
        CreateMap<PatchReservationDto, Reservation>()
            .IgnoreNullMembers();
    }
}
