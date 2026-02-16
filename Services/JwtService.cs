using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserApprovalApi.Models;

namespace UserApprovalApi.Services
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int ExpiresMinutes { get; set; } = 120;
    }

    public class JwtService
    {
        private readonly JwtOptions _opt;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly SigningCredentials _creds;

        public JwtService(IOptions<JwtOptions> opt)
        {
            _opt = opt.Value ?? throw new ArgumentNullException(nameof(opt));

            if (string.IsNullOrWhiteSpace(_opt.Key))
                throw new InvalidOperationException("Jwt:Key must be configured and non-empty.");

            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
            _creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        }

        public string CreateToken(User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));

            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                // Subject / identity
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
                new Claim("nameid", user.UserId), // keep if you reference it elsewhere
                new Claim("name", user.Name ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),

                // Authorization
                new Claim("role", user.Role.ToString()),
                new Claim("status", user.Status.ToString()),

                // Token housekeeping
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpoch(now).ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,               // "Company"
                audience: _opt.Audience,           // "FrontendApp"
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_opt.ExpiresMinutes),
                signingCredentials: _creds
            );

            var handler = new JwtSecurityTokenHandler();
            // Keep your custom claim names as-is (since MapInboundClaims = false)
            handler.OutboundClaimTypeMap.Clear();

            return handler.WriteToken(token);
        }

        private static long ToUnixEpoch(DateTime dateTimeUtc)
        {
            // Ensure it's UTC before converting
            if (dateTimeUtc.Kind != DateTimeKind.Utc)
                dateTimeUtc = DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc);

            return (long)Math.Floor((dateTimeUtc - DateTime.UnixEpoch).TotalSeconds);
        }
    }
}