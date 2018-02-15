using Demo.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Environment.CurrentDirectory);
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddEnvironmentVariables();

            var config = builder.Build();

            var factory = new ConnectionFactory() { HostName = config["Services:RabbitMQ"] };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "tasks", durable: true, exclusive: false, autoDelete: false, arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        Console.WriteLine("[Start: " + DateTime.UtcNow.ToString("yyyy:MM:dd HH:mm:ss:ffffzzz") + "]");
                        var watch = new Stopwatch();
                        watch.Start();

                        try
                        {
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);

                            Console.WriteLine(message);
                            ProcessMessage(message).Wait();
                            channel.BasicAck(ea.DeliveryTag, multiple: false);

                            watch.Stop();
                            Console.WriteLine("[Success]");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[Fail]");
                            Console.WriteLine(ex);
                            throw;
                        }
                        finally
                        {
                            watch.Stop();
                            Console.WriteLine("[" + DateTime.UtcNow.ToString("yyyy:MM:dd HH:mm:ss:ffffzzz") + ", " + watch.ElapsedMilliseconds + "ms.]");
                        }
                    };
                    channel.BasicConsume(queue: "tasks", autoAck: false, consumer: consumer);

                    Loop().Wait();
                }
            }
        }

        private static async Task Loop()
        {
            while (true)
            {
                await Task.Delay(1000);
            }
        }

        private static async Task ProcessMessage(string message)
        {
            var json = JObject.Parse(message);
            var id = json["value"].Value<int>();

            using (var db = new DemoContext())
            {
                var task = await db.Tasks.Where(t => t.Id == id).FirstOrDefaultAsync();
                if (task != null)
                {
                    task.Status = WorkerTaskStatus.Running;

                    for (var t = 1; t <= 100; t++)
                    {
                        task.Progress = t;
                        await db.SaveChangesAsync();
                    }

                    task.Status = WorkerTaskStatus.Completed;
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
