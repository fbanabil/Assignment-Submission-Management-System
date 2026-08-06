using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Backend.Helpers
{
    public record UserPayload(string UserId, string FullName, string Email, List<string> Roles);
    
    
    public interface IAuthenticationHelper
    {
        Task<string> CreateJwtToken(UserPayload payload);
        Task<string> CreateRefreshTokenAsync();
        Task<string> HashTokenAsync(string token);
        Task<bool> VerifyRefreshTokenAsync(string token, string hashedToken);
    }


    public class AuthenticationHelper : IAuthenticationHelper
    {
        private readonly IConfiguration _configuration;

        public AuthenticationHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<string> CreateJwtToken(UserPayload payload)
        {
            using var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportFromPem(_configuration["JwtSettings:PrivateKey"]);

            var key = new RsaSecurityKey(rsa.ExportParameters(true));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, payload.UserId),
                new Claim(JwtRegisteredClaimNames.UniqueName, payload.FullName),
                new Claim(JwtRegisteredClaimNames.Email, payload.Email),
            };
            claims.AddRange(payload.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Task.FromResult(jwt);
        }



        public async Task<string> CreateRefreshTokenAsync()
        {

            var bytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
            return await Task.FromResult(token);
        }

        public async Task<string> HashTokenAsync(string token)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            var hashedToken = Convert.ToBase64String(hashBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
            return await Task.FromResult(hashedToken);
        }

        public async Task<bool> VerifyRefreshTokenAsync(string token, string hashedToken)
        {
            var expectedHashedToken = await HashTokenAsync(token);

            return CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(expectedHashedToken),
                System.Text.Encoding.UTF8.GetBytes(hashedToken)
            );
        }
    }
}
