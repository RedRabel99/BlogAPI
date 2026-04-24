using BlogAPI.Application.DTOs.Tags;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Mapping;

public static class TagMappers
{
    public static TagDto ToDto(this Tag tag)
    {
        return new TagDto
        {
            TagName = tag.TagName,
            Slug = tag.Slug
        };
    }
}
