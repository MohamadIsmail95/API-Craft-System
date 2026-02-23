namespace ApiCraftSystem.Test
{
    public class CabinetResponseDto
    {
        public List<CabinetModelDto> Model { get; set; } = new();
        public List<CabinetTypeDto> Type { get; set; } = new();
    }
}
