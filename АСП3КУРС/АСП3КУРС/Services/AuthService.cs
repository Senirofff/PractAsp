using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using АСП3курс.DTOs;
using АСП3курс.Models;
using АСП3курс.Repositories;
using АСП3курс.Security;

namespace АСП3курс.Services;

public sealed class AuthService(
    IRepository<User> users, IRepository<Role> roles, IPasswordHasher<User> hasher, JwtSettings jwt)
{
    public async Task<AccessTokenDto?> RegisterAsync(RegisterDto dto, CancellationToken ct)
    {
        var role = await roles.Query().FirstAsync(r => r.Title == Access.User, ct);
        var user = new User
        {
            Login = dto.Login,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IdRole = role.IdRole,
            IsActive = true
        };
        user.PasswordHash = hasher.HashPassword(user, dto.Password);
        await users.AddAsync(user, ct);
        try
        {
            await users.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return null;
        }

        return IssueToken(user, Access.User);
    }

    public async Task<AccessTokenDto?> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        var user = await users.Query().Include(u => u.IdRoleNavigation)
            .FirstOrDefaultAsync(u => u.Login == dto.Login, ct);

        if (user is null || !user.IsActive)
            return null;

        PasswordVerificationResult result;
        try
        {
            result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        }
        catch (FormatException)
        {
            return null;
        }

        if (result == PasswordVerificationResult.Failed)
            return null;

        return IssueToken(user, user.IdRoleNavigation.Title);
    }

    private AccessTokenDto IssueToken(User user, string role)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(30);
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience,
            [new Claim(JwtRegisteredClaimNames.Sub, user.IdUser.ToString(CultureInfo.InvariantCulture)),
             new Claim("role", role)], now, expires,
            new SigningCredentials(jwt.Key, SecurityAlgorithms.HmacSha256));

        return new AccessTokenDto(new JwtSecurityTokenHandler().WriteToken(token), user.IdUser, role, expires);
    }
}
