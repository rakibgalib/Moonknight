using AutoMapper;
using MoonKnight.Identity.Application.Dtos.User;
using MoonKnight.Identity.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MoonKnight.Identity.Mapping
{
    public class IdentityMappingProfile : Profile
    {
        public IdentityMappingProfile()
        {
            CreateMap<UserCreateDto, UserProfile>();
            CreateMap<UserProfile, UserDto>();
        }
    }
}
