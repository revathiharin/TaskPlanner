using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TaskPlanner.Data;
using TaskPlanner.Models;

namespace TaskPlanner.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var projects = _context.Projects.Include(p => p.Tasks).ToList();
            return View(projects);
        }

        [HttpPost]
        public IActionResult Create(string name)
        {
            var project = new Project
            {
                Name = name,
                SortBy = "A-Z"  // Set SortBy to "A-Z"
            };
            _context.Projects.Add(project);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateSortBy([FromBody] UpdateSortByRequest request)
        {
            Console.WriteLine("Received projectId: " + request.ProjectId + ", sortBy: " + request.SortBy);

            var project = _context.Projects.Find(request.ProjectId);
            if (project != null)
            {
                project.SortBy = request.SortBy;
                _context.SaveChanges();
                Console.WriteLine("Project updated successfully.");
            }
            else
            {
                Console.WriteLine("Project not found.");
                return NotFound("Project not found");
            }

            return Ok();
        }


    }
}
