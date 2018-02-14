namespace Demo.Database
{
    public class WorkerTask
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public WorkerTaskStatus Status { get; set; }
        public int Progress { get; set; }
    }
}
