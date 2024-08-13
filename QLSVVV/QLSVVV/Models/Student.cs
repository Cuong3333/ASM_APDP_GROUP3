namespace QLSVVV.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public DateTime? DoB { get; set; }
        public string? Address { get; set; }
        public string? Major { get; set; }
        public ICollection<CourseStudent> CourseStudent { get; set; } = new List<CourseStudent>();
        public Student()
        {
        }
    }
}
