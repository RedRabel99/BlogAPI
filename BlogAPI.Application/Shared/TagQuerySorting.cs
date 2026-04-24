using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Shared
{
    public class TagQuerySorting(string? sortOrder, string? sortColumn) : IQuerySorting<Tag>
    {
        public string? SortColumn { get; } = sortColumn;
        public SortingOrder SortOrder { get; } = sortOrder?.ToLower() == "asc" ? SortingOrder.Ascending : SortingOrder.Descending;
        public Expression<Func<Tag, object>> GetSortProperty()
        {
            return SortColumn.ToLower() switch
            {
                "tagname" => t => t.TagName,
                "slug" => t => t.Slug,
                _ => t => t.Id
            };
        }
    }
}
