using BlogAPI.Application.Common.Errors;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Common.Persistance;
using BlogAPI.Application.Auth;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace BlogAPI.Infrastructure.Services
{
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IAppDbContext _appDbContext;

        public RefreshTokenService(IOptions<JwtOptions> jwtOptions, AppDbContext appDbContext)
        {
            _jwtOptions = jwtOptions.Value;
            _appDbContext = appDbContext;
        }

        public async Task<string> IssueAsync(string applicationUserId, CancellationToken ct)
        {
            var tokenRaw = NewTokenRaw();

            var hashedToken = Hash(tokenRaw);

            var refreshToken = new RefreshToken
            {
                ApplicationUserId = applicationUserId,
                TokenHash = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
            };

            _appDbContext.RefreshTokens.Add(refreshToken);
            await _appDbContext.SaveChangesAsync(ct);
            return tokenRaw;
        }

        public async Task RevokeAsync(string refreshToken, CancellationToken ct)
        {
            var hash = Hash(refreshToken);
            var token = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);

            if (token is null || token.RevokedAt is not null) return;

            token.RevokedAt = DateTime.UtcNow;
            await _appDbContext.SaveChangesAsync(ct);
        }

        public async Task<Result<RefreshRotationResult>> RotateAsync(string refreshToken, CancellationToken ct)
        {
            var oldHash = Hash(refreshToken);
            var oldToken = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == oldHash, ct);

            if(oldToken is null)
            {
                return Result<RefreshRotationResult>.Failure(RefreshTokenErrors.Invalid);
            }

            if(!oldToken.IsActive)
            {
                return Result<RefreshRotationResult>.Failure(RefreshTokenErrors.Invalid);
            }

            var userId = oldToken.ApplicationUserId;
            oldToken.RevokedAt = DateTime.UtcNow;

            var newTokenRaw = NewTokenRaw();
            var newHash = Hash(newTokenRaw);

            var newToken = new RefreshToken
            {
                ApplicationUserId = userId,
                TokenHash = newHash,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
            };
            //For now we dont connect old token to new one, but in the future replacedByTokenId will be added for reuse detection
            _appDbContext.RefreshTokens.Add(newToken);
            await _appDbContext.SaveChangesAsync(ct);
            return Result<RefreshRotationResult>.Success(
                new RefreshRotationResult(NewRefreshToken: newTokenRaw, ApplicationUserId: userId
            ));
        }

        private static string Hash(string raw) => 
            Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

        private static string NewTokenRaw() => 
            Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
    }
}
