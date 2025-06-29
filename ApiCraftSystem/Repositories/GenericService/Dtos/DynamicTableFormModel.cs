using ApiCraftSystem.Helper.Enums;
using System.ComponentModel.DataAnnotations;

namespace ApiCraftSystem.Repositories.GenericService.Dtos
{
    public class DynamicTableFormModel
    {
        [Required(ErrorMessage = "Connection string is required.")]
        public string ConnectionString { get; set; } = string.Empty;

        [Required(ErrorMessage = "Provider is required.")]
        public DatabaseType SelectedProvider { get; set; }

        [Required(ErrorMessage = "Table name is required.")]
        public string TableName { get; set; } = string.Empty;

        public List<string>? UserIds { get; set; } = new List<string>();

        public string? DateFilterColumnName { get; set; } = string.Empty;

    }
}
