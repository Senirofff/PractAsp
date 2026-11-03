using AutoMapper;
using АСП3курс.DTOs;
using АСП3курс.Models;

namespace АСП3курс.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products.Count));
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();
        CreateMap<PatchCategoryDto, Category>()
            .IgnoreNullMembers();

        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Category, o => o.MapFrom(s => s.IdCategoryNavigation));
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
        CreateMap<PatchProductDto, Product>()
            .IgnoreNullMembers();

        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.IdRoleNavigation));
        CreateMap<UpdateProfileDto, User>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
        CreateMap<PatchUserDto, User>()
            .IgnoreNullMembers();

        CreateMap<Role, RoleDto>();
        CreateMap<CreateRoleDto, Role>();
        CreateMap<UpdateRoleDto, Role>();
        CreateMap<PatchRoleDto, Role>()
            .IgnoreNullMembers();

        CreateMap<Cart, CartDto>()
            .ForMember(d => d.User, o => o.MapFrom(s => s.IdUserNavigation))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.CartItems));
        CreateMap<CreateCartDto, Cart>();
        CreateMap<UpdateCartDto, Cart>();
        CreateMap<PatchCartDto, Cart>()
            .IgnoreNullMembers();

        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.Product, o => o.MapFrom(s => s.IdProductNavigation));
        CreateMap<CreateCartItemDto, CartItem>();
        CreateMap<UpdateCartItemDto, CartItem>();
        CreateMap<PatchCartItemDto, CartItem>()
            .IgnoreNullMembers();

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.User, o => o.MapFrom(s => s.IdUserNavigation))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.OrderItems));
        CreateMap<CreateOrderDto, Order>();
        CreateMap<UpdateOrderDto, Order>();
        CreateMap<PatchOrderDto, Order>()
            .IgnoreNullMembers();

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.Product, o => o.MapFrom(s => s.IdProductNavigation));
        CreateMap<CreateOrderItemDto, OrderItem>();
        CreateMap<UpdateOrderItemDto, OrderItem>();
        CreateMap<PatchOrderItemDto, OrderItem>()
            .IgnoreNullMembers();

        CreateMap<Reservation, ReservationDto>()
            .ForMember(d => d.User, o => o.MapFrom(s => s.IdUserNavigation))
            .ForMember(d => d.Product, o => o.MapFrom(s => s.IdProductNavigation));
        CreateMap<CreateReservationDto, Reservation>();
        CreateMap<UpdateReservationDto, Reservation>();
        CreateMap<PatchReservationDto, Reservation>()
            .IgnoreNullMembers();
    }
}
