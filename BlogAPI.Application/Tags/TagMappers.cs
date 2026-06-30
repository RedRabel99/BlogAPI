using System.Linq.Expressions;
using BlogAPI.Application.Tags.Dtos;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Tags;

public static class TagMappers
{
    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto
        {
            TagName = tag.TagName,
        };
    }

    public static Expression<Func<Tag, TagDto>> ProjectToDto =>
        tag => new TagDto()
        {
            TagName = tag.TagName
        };
}
