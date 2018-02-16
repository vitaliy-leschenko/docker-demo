using System;

namespace Demo.Web.Models
{
    public class TaskModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public int Status { get; set; }
        public int Progress { get; set; }
        public DateTimeOffset? Created { get; set; }
        public DateTimeOffset? Started { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
