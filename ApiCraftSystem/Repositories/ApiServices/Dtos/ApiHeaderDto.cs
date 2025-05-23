using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCraftSystem.Repositories.ApiServices.Dtos
{
    public class ApiHeaderDto
    {
        public Guid Id { get; set; }
        public Guid ApiStoreId { get; set; }
        [Required]
        public string HeaderKey { get; set; } = string.Empty;
        [Required]
        public string HeaderValue { get; set; } = string.Empty;
    }
}
