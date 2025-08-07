using AutoMapper;
using MoonKnight.Identity.Domain.Entities;
using MoonKnight.Identity.Dtos.User;
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
