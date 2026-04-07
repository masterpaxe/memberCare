using MemberCare.Api.Contracts;
using Dapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MemberCare.Api.Services;

public sealed class AuthService
{
    private readonly IConfiguration _configuration;
    private readonly SqlConnectionFactory _connectionFactory;

    public AuthService(IConfiguration configuration, SqlConnectionFactory connectionFactory)
    {
        _configuration = configuration;
        _connectionFactory = connectionFactory;
    }

    public AuthTokenResponse Login(AuthLoginRequest request)
    {
        // TODO: Validate credentials against database
        // For now, accept any non-empty username/password
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Username and password are required");
        }

        var token = GenerateJwtToken(request.Username);
        return new AuthTokenResponse(
            AccessToken: token,
            RefreshToken: token, // TODO: Implement refresh token rotation
            ExpiresIn: int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60") * 60
        );
    }

    private string GenerateJwtToken(string username)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT secret key not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "membercare-api";
        var audience = _configuration["Jwt:Audience"] ?? "membercare-client";
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        // TODO: Load user role and branch from database
        // For demo, assign role based on username pattern
        var role = username.ToLower() switch
        {
            "admin" => "super_admin",
            "pastor" => "pastor",
            "visitorcare" => "follow_up_officer",
            "attendance" => "attendance_officer",
            "finance" => "finance_officer",
            "reports" => "report_viewer",
            _ => "church_admin" // default for unknown users
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var branchId = ResolveDefaultBranchId();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.Name, username),
            new Claim("role", role),
            // Development default branch selection from seeded branches table.
            new Claim("branch_id", branchId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private Guid ResolveDefaultBranchId()
    {
        try
        {
            using var conn = _connectionFactory.CreateOpenConnection();
            var branchId = conn.ExecuteScalar<Guid?>("SELECT branch_id FROM branches ORDER BY name LIMIT 1");
            return branchId ?? Guid.Empty;
        }
        catch
        {
            return Guid.Empty;
        }
    }
}
