namespace ApiCraftSystem.Test
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime HireDate { get; set; }

        public Employee() { }

        public Employee(int id, string name, string description, int age, DateTime hireDate)
        {
            Id = id;
            Name = name;
            Description = description;
            Age = age;
            HireDate = hireDate;

        }
    }
}
