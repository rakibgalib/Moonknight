﻿namespace MoonKnight.Identity.Application.Dtos.User
{
    public class UserCreateDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public Guid TenantId { get; set; }
    }
}
