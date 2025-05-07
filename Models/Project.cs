namespace TaskPlanner.Models
{
    public class Project
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string SortBy { get; set; }
        public virtual ICollection<TaskList> Tasks { get; set; }
    }
    public class UpdateSortByRequest
    {
        public int ProjectId { get; set; }
        public string SortBy { get; set; }
    }
}
