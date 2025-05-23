using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCraftSystem.Repositories.ApiServices.Dtos
{
    public class ApiMapDto
    {
        public Guid Id { get; set; }

        public Guid ApiStoreId { get; set; }

        [Required]
        public string FromKey { get; set; } = string.Empty;

        [Required]
        public string MapKey { get; set; } = string.Empty;

        [Required]
        public string DataType { get; set; } = string.Empty;


    }
}
