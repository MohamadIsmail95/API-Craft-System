namespace ApiCraftSystem.Repositories.AccountService.Dtos
{
    public class UpdateUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string? RoleId { get; set; }
        public Guid? TenantId { get; set; }

        public UpdateUserDto() { }

        public UpdateUserDto(string id, string userName, string? email, string? phoneNumber,
           string? roleId, Guid? tenantId)
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
