using System.Linq.Expressions;
using BlogAPI.Application.Extensions;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Shared
{
    public class TagQuerySorting(string? sortOrder, string? sortColumn) : IQuerySorting<Tag>
    {
        private string? SortColumn { get; } = sortColumn;

        private SortingOrder SortOrder { get; } = sortOrder?.ToLower() switch
        {
            "asc" => SortingOrder.Ascending,
            "desc" => SortingOrder.Descending,
            _ => SortingOrder.Ascending
        };

        private Expression<Func<Tag, object>> SortProperty 
            => SortColumn?.ToLower() switch
            {
                "tagname" => t => t.TagName,
                "slug" => t => t.Slug,
                _ => t => t.Id
            };

        public IQueryable<Tag> Apply(IQueryable<Tag> query)
        {
            return query.OrderByDirection(SortProperty, SortOrder);
        }
    }
}
