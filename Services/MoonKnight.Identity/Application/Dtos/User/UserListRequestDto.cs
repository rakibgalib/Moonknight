namespace MoonKnight.Identity.Application.Dtos.User
{
    public class UserListRequestDto
    {
        public Guid? TenantId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
