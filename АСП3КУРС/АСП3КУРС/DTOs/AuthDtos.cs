using System.ComponentModel.DataAnnotations;

namespace АСП3курс.DTOs;

public record LoginDto(
    [Required, StringLength(50)] string Login,
    [Required, StringLength(128)] string Password);

public record RegisterDto(
    [Required, StringLength(50), RegularExpression(@"[a-zA-Z0-9_.-]+")] string Login,
    [Required, StringLength(128, MinimumLength = 8)] string Password,
    [Required, StringLength(50)] string FirstName,
    [Required, StringLength(50)] string LastName);

public record AccessTokenDto(string AccessToken, int UserId, string Role, DateTime ExpiresAt);

public record UpdateProfileDto(
    [Required, StringLength(50)] string FirstName,
    [Required, StringLength(50)] string LastName);
