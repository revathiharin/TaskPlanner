using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Data;
using TaskPlanner.Models;

namespace TaskPlanner.Controllers
{

    // DTO for task sorting
    public class TaskSortOrderDto
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
    }
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateTask(int projectId, string title)
        {
            // Get the current maximum SortOrder for the specified project
            var currentMaxSortOrder = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Max(t => (int?)t.SortOrder) ?? 0; // Use null-coalescing operator to default to 0 if no tasks exist

            // Create a new task with SortOrder set to currentMaxSortOrder + 1
            var task = new TaskList
            {
                ProjectId = projectId,
                Title = title,
                SortOrder = currentMaxSortOrder + 1
            };

            _context.Tasks.Add(task);
            _context.SaveChanges();

            return RedirectToAction("Index", "Projects");
        }

        [HttpPost]
        public IActionResult UpdateCompletion(int id, bool isCompleted)
        {
            var task = _context.Tasks.Find(id);
            if (task != null)
            {
                task.IsCompleted = isCompleted;
                _context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult MoveTask(int id, int projectId)
        {
            var task = _context.Tasks.Find(id);
            if (task != null)
            {
                task.ProjectId = projectId;
                _context.SaveChanges();
            }
            return Ok();
        }

        [HttpGet]
        public JsonResult GetTasks(int projectId)
        {
            // Fetch the SortBy value from the Projects table based on the projectId
            var sortBy = _context.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new { p.SortBy }).ToList()
                .FirstOrDefault(); // Get the SortBy value for the specific project


            var tasks = _context.Tasks
                        .Where(t => t.ProjectId == projectId)
                        .OrderBy(t => t.SortOrder)
                        .Select(t => new { t })
                        .ToList();
            Console.WriteLine("GetTasks called. SortBy: " + sortBy + ", Tasks Count: " + tasks.Count);
            return Json(new { SortBy = sortBy, Tasks = tasks });
        }

        [HttpPost]
        public IActionResult ReorderAndMoveTasks([FromBody] TaskReorderDto dto)
        {
            var taskIds = dto.Tasks.Select(t => t.Id).ToList();
            var tasks = _context.Tasks.Where(t => taskIds.Contains(t.Id)).ToList();

            foreach (var task in tasks)
            {
                var updatedInfo = dto.Tasks.First(t => t.Id == task.Id);
                task.SortOrder = updatedInfo.SortOrder;
                task.ProjectId = updatedInfo.ProjectId;
            }

            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult MoveTaskToProject(int taskId, int newProjectId)
        {
            Console.WriteLine($"[MoveTaskToProject] Invoked - TaskId: {taskId}, NewProjectId: {newProjectId}");
            var task = _context.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null) return NotFound();

            var maxSortOrder = _context.Tasks
                .Where(t => t.ProjectId == newProjectId)
                .Max(t => (int?)t.SortOrder) ?? -1;

            task.ProjectId = newProjectId;
            task.SortOrder = maxSortOrder + 1;

            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult UpdateSortOrder([FromBody] UpdateSortOrderRequest request)
        {
            if (request.tasks == null || !request.tasks.Any())
            {
                return Json(new { success = false, message = "No tasks provided." });
            }

            // Update the SortOrder for each task
            foreach (var task in request.tasks)
            {
                var existingTask = _context.Tasks.Find(task.Id);
                if (existingTask != null)
                {
                    existingTask.SortOrder = task.SortOrder; // Update the SortOrder
                }
            }

            // Save changes to the database
            _context.SaveChanges();

            return Json(new { success = true });
        }



        /*
                [HttpPost]
                public IActionResult ReorderAndMoveTasks([FromBody] TaskReorderDto dto)
                {
                    var tasksToUpdate = _context.Tasks
                        .Where(t => dto.Tasks.Select(x => x.Id).Contains(t.Id))
                        .ToList();

                    foreach (var task in tasksToUpdate)
                    {
                        var newInfo = dto.Tasks.First(x => x.Id == task.Id);
                        task.SortOrder = newInfo.SortOrder;
                        task.ProjectId = dto.ProjectId; // This moves it if it's a cross-project drop
                    }

                    _context.SaveChanges();
                    return Ok();
                }*/

    }

    public class UpdateSortOrderRequest
    {
        public List<TaskDto> tasks { get; set; }
    }

    public class TaskDto
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
    }
}

