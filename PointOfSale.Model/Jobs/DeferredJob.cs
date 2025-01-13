using static PointOfSale.Model.Enum;

namespace PointOfSale.Model.Jobs
{
    public class DeferredJob
    {
        public int JobId { get; set; }
        public string JobName { get; set; }
        public JobTypeEnum JobType { get; set; }
        public DateTime ScheduleTime { get; set; }
        public DateTime? LastExecutionTime { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
        public bool? Result { get; set; }
        public string TaskData { get; set; }
        public string? Error { get; set; }
    }
}
