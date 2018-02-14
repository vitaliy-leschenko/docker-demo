using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Demo.Database
{
    public class DemoContext: DbContext
    {
        public DemoContext(): base()
        {
        }

        public DemoContext(DbContextOptions<DemoContext> options): base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Environment.CurrentDirectory);
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddEnvironmentVariables();

            var config = builder.Build();
            var connection = config["Database:ConnectionString"];
            optionsBuilder.UseSqlServer(connection);
        }

        public DbSet<WorkerTask> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WorkerTask>().HasKey(t => t.Id);
            modelBuilder.Entity<WorkerTask>().Property(t => t.Id).ValueGeneratedOnAdd();
        }
    }
}
