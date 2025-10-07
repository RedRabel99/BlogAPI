using BlogAPI.Application.DTOs;
using BlogAPI.Application.Shared;
using BlogAPI.Domain.Entities;
using System.Linq.Expressions;

namespace BlogAPI.Application.Extensions;

public static class UserProfileQueryExtensions
{
    public static IQueryable<UserProfile> ApplyFiltering(
        this IQueryable<UserProfile> query,
        UserProfileQueryParameters queryParameters)
    {
        if (!string.IsNullOrWhiteSpace(queryParameters.UserName))
        {
            query = query.Where(u =>
                u.UserName.Contains(
                    queryParameters.UserName,
                    StringComparison.InvariantCultureIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(queryParameters.DisplayName))
        {
            query = query.Where(u =>
                u.DisplayName.Contains(
                    queryParameters.DisplayName,
                    StringComparison.InvariantCultureIgnoreCase));
        }

        return query;
    }

    public static IQueryable<UserProfile> ApplySorting(
        this IQueryable<UserProfile> query,
        UserProfileQueryParameters queryParameters)
    {
        if (string.IsNullOrEmpty(queryParameters.SortColumn))
        {
            return query;
        }

        var keySelector = GetSortProperty(queryParameters);

        return queryParameters.SortingOrder == SortingOrder.Ascending
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);
    }

    private static Expression<Func<UserProfile, object>> GetSortProperty(UserProfileQueryParameters queryParameters)
    {
        return queryParameters.SortColumn?.ToLower() switch
        {
            "username" => userProfile => userProfile.UserName,
            "displayname" => userProfile => userProfile.DisplayName,
            _ => userProfile => userProfile.Id
        };
    }
}
