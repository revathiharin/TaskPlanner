namespace TaskPlanner.Models
{
    public class TaskList
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public int SortOrder { get; set; }
        public virtual Project Project { get; set; }
    }
}
