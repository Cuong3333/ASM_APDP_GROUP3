namespace QLSVVV.Models
{
    public class Course
    {
        public int Id { get; set; }
        public String? Name { get; set; }
        public string? Class { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string? Major { get; set; }
        public string? Lecturer { get; set; }
        public ICollection<CourseStudent> CourseStudent { get; set; } = new List<CourseStudent>();
        public Course()
        {
        }
    }
}
