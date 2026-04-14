using AutoMapper;
using BlogAPI.Application.Mapping;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.DTOs;

public class UpdateUserProfileDto : IMapFrom<UserProfile>
{
    public string DisplayName { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserProfile, UpdateUserProfileDto>().ReverseMap();
    }
}
