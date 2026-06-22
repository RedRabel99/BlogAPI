using BlogAPI.Application.Constants;
using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Application.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogAPI.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _jwtOptions = options.Value;
        }

        public Task<AccessTokenResultDto> GenerateAccessTokenAsync(IUserInfo user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(AppClaimTypes.UserProfileId, user.UserProfileId),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessTokenMinutes = _jwtOptions.AccessTokenMinutes;

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenMinutes),
                signingCredentials: creds);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Task.FromResult(new AccessTokenResultDto(accessToken, accessTokenMinutes * 60));
        }
    }
}
