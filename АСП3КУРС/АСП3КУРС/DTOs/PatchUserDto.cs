namespace АСП3курс.DTOs;

public class PatchUserDto
{
    public string? Login { get; set; }
    public string? PasswordHash { get; set; }
    public int? IdRole { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
    public int? DiscountPercent { get; set; }
}
