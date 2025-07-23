using AutoMapper;
using MoonKnight.Auth.Domain.Entities;
using MoonKnight.Auth.Dtos;

namespace MoonKnight.Auth.Mappings
{
    public class AuthMappingProfile: Profile
    {
        public AuthMappingProfile()
        {
            // User -> UserDto
            CreateMap<User, UserDto>();

            // RegisterDto -> User (we will hash password separately)
       

            // RegisterDto → User (we’ll still hash password separately)
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                  .ForMember(dest => dest.TenantId, opt => opt.Ignore());

        }
    }
}
