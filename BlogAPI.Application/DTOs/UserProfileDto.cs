using AutoMapper;
using BlogAPI.Application.Mapping;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.DTOs;

public record UserProfileDto : IMapFrom<UserProfile>
{
    public Guid Id { get; set; }
    public string ApplicationUserId { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserProfile, UserProfileDto>().ReverseMap();
    }
}
