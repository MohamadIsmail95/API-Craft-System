namespace ApiCraftSystem.Repositories.RateService.Dtos
{
    public class RateDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public int Grade { get; set; } = 0;

        public RateDto() { }
        public RateDto(Guid id, string? userId, int grade)
        {
            Id = id;
            UserId = userId;
            Grade = grade;
        }
    }
}
