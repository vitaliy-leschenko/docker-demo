using System;
using System.Linq;
using System.Threading.Tasks;
using Demo.Database;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Web.Controllers
{
    [Produces("application/json")]
    public class TasksController : Controller
    {
        private readonly IServiceProvider provider;

        public TasksController(IServiceProvider provider)
        {
            this.provider = provider;
        }

        [HttpGet]
        [Route("api/tasks")]
        public async Task<TaskModel[]> Get()
        {
            using (var db = provider.GetService<DemoContext>())
            {
                var query = from task in db.Tasks
                            select new TaskModel
                            {
                                Id = task.Id,
                                Status = (int)task.Status,
                                Progress = task.Progress
                            };
                return await query.ToArrayAsync();
            }
        }
    }
}