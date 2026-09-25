using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.DataAccess.Repositories;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ISystemAccountRepository _accountRepository;

    public AuthService(IConfiguration configuration, ISystemAccountRepository accountRepository)
    {
        _configuration = configuration;
        _accountRepository = accountRepository;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var inputEmail = request.Email.Trim().ToLower();
        var inputPassword = request.Password.Trim();

        // 1. Check Admin from appsettings.json
        var adminEmail = (_configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org").Trim().ToLower();
        var adminPassword = (_configuration["AdminAccount:Password"] ?? "@@abc123@@").Trim();
        var adminRole = _configuration["AdminAccount:Role"] ?? "Admin";

        if (inputEmail == adminEmail && inputPassword == adminPassword)
        {
            var token = GenerateJwtToken(0, "Administrator", adminEmail, adminRole, 0);
            return new LoginResponseDto
            {
                Token = token,
                AccountId = 0,
                AccountName = "Administrator",
                Email = adminEmail,
                Role = adminRole,
                AccountRole = 0
            };
        }

        // 2. Check SystemAccount in Database
        var account = await _accountRepository.GetAccountByEmailAsync(inputEmail);
        if (account == null || account.AccountPassword != inputPassword)
        {
            throw new BusinessRuleException("Invalid email or password.");
        }

        string roleName = account.AccountRole switch
        {
            1 => "Staff",
            2 => "Lecturer",
            _ => "Guest"
        };

        var jwtToken = GenerateJwtToken(
            account.AccountId, 
            account.AccountName ?? "User", 
            account.AccountEmail ?? inputEmail, 
            roleName, 
            account.AccountRole ?? 0);

        return new LoginResponseDto
        {
            Token = jwtToken,
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? "User",
            Email = account.AccountEmail ?? inputEmail,
            Role = roleName,
            AccountRole = account.AccountRole
        };
    }

    public async Task<SystemAccountDto?> GetCurrentUserAsync(short accountId, string role)
    {
        if (role == "Admin" || accountId == 0)
        {
            return new SystemAccountDto
            {
                AccountId = 0,
                AccountName = "Administrator",
                AccountEmail = _configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org",
                AccountRole = 0
            };
        }

        var account = await _accountRepository.GetAccountByIdAsync(accountId);
        if (account == null) return null;

        return new SystemAccountDto
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            AccountRole = account.AccountRole
        };
    }

    private string GenerateJwtToken(short accountId, string accountName, string email, string role, int accountRole)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "FUNewsManagementSystem_SuperSecretKey_2024_SecurityToken_@123456";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "FUNewsManagementSystem_BE";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "FUNewsManagementSystem_FE";
        var expireMinutes = int.TryParse(_configuration["Jwt:ExpireMinutes"], out var exp) ? exp : 120;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Name, accountName),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("AccountRole", accountRole.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
