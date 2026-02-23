using System.Text.Json.Serialization;

namespace ApiCraftSystem.Test
{
    public class CabinetModelDto
    {
        public string Name { get; set; } = string.Empty;
        public string? BATTERRYTYPE { get; set; }
        public string? CABINETPOWERLIBRARY { get; set; }
        public int Id { get; set; }
        public int NUmberOfPSU { get; set; }
        public string? RENEWABLECABINETTYPE { get; set; }
        public float SpaceInstallation { get; set; }
        public string? TPVersion { get; set; }

        [JsonPropertyName("Other Battery Type")]
        public string? OtherBatteryType { get; set; }

        [JsonPropertyName("Bat Capacity")]
        public string? BatCapacity { get; set; }
    }
}
