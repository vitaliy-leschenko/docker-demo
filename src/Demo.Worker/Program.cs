using Demo.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.Worker
{
    class Program
    {
        private static readonly AutoResetEvent closing = new AutoResetEvent(false);

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

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        Console.WriteLine(CurrentDate() + " [Start]");

                        try
                        {
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);

                            await ProcessMessage(message);

                            channel.BasicAck(ea.DeliveryTag, multiple: false);
                            Console.WriteLine(CurrentDate() + " [Success]");
                        }
                        catch (Exception ex)
                        {
                            channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);

                            Console.WriteLine(CurrentDate() + " [Fail]");
                            Console.WriteLine(ex);
                        }
                    };
                    channel.BasicConsume(queue: "tasks", autoAck: false, consumer: consumer);

                    Console.WriteLine("Press Ctrl+C to exit.");
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
                    closing.WaitOne();
                }
            }

            string CurrentDate()
            {
                return DateTime.UtcNow.ToString("yyyy:MM:dd HH:mm:ss:ffffzzz");
            }
        }

        private static void OnExit(object sender, ConsoleCancelEventArgs e)
        {
            closing.Set();
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

                    for (var t = 0; t <= 100; t+=10)
                    {
                        task.Progress = t;
                        await db.SaveChangesAsync();
                    }

                    task.Progress = 100;
                    task.Status = WorkerTaskStatus.Completed;
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
