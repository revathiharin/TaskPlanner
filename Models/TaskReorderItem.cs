namespace TaskPlanner.Models
{
    public class TaskReorderItem
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public int ProjectId { get; set; }
    }
}
