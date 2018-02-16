using System;

namespace Demo.Database
{
    public class WorkerTask
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public WorkerTaskStatus Status { get; set; }
        public int Progress { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Started { get; set; }
        public DateTimeOffset? Updated { get; set; }
    }
}
