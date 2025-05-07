using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaskPlanner.Models;

namespace TaskPlanner.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskList> Tasks { get; set; }
    }
}
