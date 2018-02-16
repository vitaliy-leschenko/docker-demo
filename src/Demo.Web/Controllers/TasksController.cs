using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo.Database;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;

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
                                Comment = task.Comment,
                                Status = (int)task.Status,
                                Progress = task.Progress,
                                Created = task.Created,
                                Updated = task.Updated,
                                Started = task.Started
                            };
                return await query.ToArrayAsync();
            }
        }

        [HttpGet]
        [Route("api/tasks/{id}")]
        public async Task<TaskModel> Get(int id)
        {
            using (var db = provider.GetService<DemoContext>())
            {
                var query = from task in db.Tasks
                            where task.Id == id
                            select new TaskModel
                            {
                                Id = task.Id,
                                Comment = task.Comment,
                                Status = (int)task.Status,
                                Progress = task.Progress,
                                Created = task.Created,
                                Updated = task.Updated,
                                Started = task.Started
                            };
                return await query.FirstOrDefaultAsync();
            }
        }

        [HttpDelete]
        [Route("api/tasks/{id}")]
        public async Task Delete(int id)
        {
            using (var db = provider.GetService<DemoContext>())
            {
                var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
                db.Tasks.Remove(task);
                await db.SaveChangesAsync();
            }
        }

        [HttpPut]
        [Route("api/tasks/{id}")]
        public async Task<TaskModel> Put(int id, [FromBody]TaskModel model)
        {
            using (var db = provider.GetService<DemoContext>())
            {
                var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
                task.Comment = model.Comment;
                task.Progress = model.Progress;
                task.Status = (WorkerTaskStatus)model.Status;

                await db.SaveChangesAsync();

                return new TaskModel
                {
                    Id = task.Id,
                    Comment = task.Comment,
                    Status = (int)task.Status,
                    Progress = task.Progress,
                    Created = task.Created,
                    Updated = task.Updated,
                    Started = task.Started
                };
            }
        }

        [HttpPost]
        [Route("api/tasks")]
        public async Task<TaskModel> Post([FromBody]TaskModel model)
        {
            var config = provider.GetService<IConfiguration>();

            using (var db = provider.GetService<DemoContext>())
            {
                var task = new WorkerTask
                {
                    Comment = model.Comment,
                    Status = WorkerTaskStatus.Pending,
                    Progress = 0,
                    Created = DateTimeOffset.UtcNow,
                    Updated = null,
                    Started = null
                };

                db.Tasks.Add(task);

                await db.SaveChangesAsync();

                var factory = new ConnectionFactory() { HostName = config["Services:RabbitMQ"] };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "tasks", durable: true, exclusive: false, autoDelete: false, arguments: null);

                        var message = new JObject(new JProperty("value", task.Id)).ToString();
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "",
                                routingKey: "tasks",
                                basicProperties: null,
                                body: body);
                    }
                }

                return new TaskModel
                {
                    Id = task.Id,
                    Comment = task.Comment,
                    Status = (int)task.Status,
                    Progress = task.Progress,
                    Created = task.Created,
                    Updated = task.Updated,
                    Started = task.Started
                };
            }
        }
    }
}