using BlogAPI.Application.Shared;
using BlogAPI.Application.Shared.UserProfile;
using BlogAPI.Domain.Entities;
using System.Linq.Expressions;

namespace BlogAPI.Application.Extensions;

public static class UserProfileQueryExtensions
{
    public static IQueryable<UserProfile> ApplyFiltering(
        this IQueryable<UserProfile> query,
        UserProfileQueryFiltering queryFilters)
    {
        if (!string.IsNullOrWhiteSpace(queryFilters.UserName))
        {
            query = query.Where(u =>
                u.Username.Contains(
                    queryFilters.UserName));
        }

        if (!string.IsNullOrWhiteSpace(queryFilters.DisplayName))
        {
            query = query.Where(u =>
                u.DisplayName.Contains(
                    queryFilters.DisplayName));
        }

        return query;
    }

    public static IQueryable<UserProfile> ApplySorting(
        this IQueryable<UserProfile> query,
        UserProfileQuerySorting querySortingParameters)
    {
        if (string.IsNullOrEmpty(querySortingParameters.SortColumn))
        {
            return query;
        }

        var keySelector = GetSortProperty(querySortingParameters);

        return querySortingParameters.SortingOrder == SortingOrder.Ascending
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);
    }

    private static Expression<Func<UserProfile, object>> GetSortProperty(UserProfileQuerySorting queryParameters)
    {
        return queryParameters.SortColumn?.ToLower() switch
        {
            "username" => userProfile => userProfile.Username,
            "displayname" => userProfile => userProfile.DisplayName,
            _ => userProfile => userProfile.Id
        };
    }
}
