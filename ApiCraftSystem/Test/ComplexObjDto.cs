namespace ApiCraftSystem.Test
{
    public class ComplexObjDto
    {
        public List<ColsDto> Cols { get; set; } = new List<ColsDto>();

        public List<RowsDto> Rows { get; set; } = new List<RowsDto>();

        public string Date { get; set; } = string.Empty;
    }

    public class ColsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;


        public ColsDto() { }

        public ColsDto(int id, string title, string dataType, string description)
        {
            Id = id;
            Title = title;
            DataType = dataType;
            Description = description;
        }
    }

    public class RowsDto
    {
        public int Id { get; set; }

        public int Count { get; set; }
    }
}
