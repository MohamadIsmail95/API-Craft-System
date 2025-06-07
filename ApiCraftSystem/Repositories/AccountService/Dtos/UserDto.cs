namespace ApiCraftSystem.Repositories.AccountService.Dtos
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? RoleId { get; set; }
        public string? RoleName { get; set; } = string.Empty;
        public Guid? TenantId { get; set; }
        public string? TenantName { get; set; } = string.Empty;

        public UserDto() { }

        public UserDto(string id, string userName, string? email, string? phoneNumber, string? roleId, Guid? tenantId)
        {
            Id = id;
            UserName = userName;
            Email = email;
            PhoneNumber = phoneNumber;
            RoleId = roleId;
            TenantId = tenantId;

        }

    }
}
