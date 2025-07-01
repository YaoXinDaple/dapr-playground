using Dapr.Actors;

namespace Rpa.Client
{
    public interface IRpaWorker : IActor
    {
        Task CreateAsync(Guid taskId);
        Task StartAsync(Guid taskId);

        Task StopAsync(Guid taskId);

        Task<string> GetCurrentTaskStateAsync(Guid taskId);

        Task<string> GetResultAsync(Guid taskId);
    }

    public class RpaWorkerData
    {
        public Guid TaskId { get; set; }
        public string TaskType { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public RpaTaskState TaskState { get; set; } = RpaTaskState.Pending;

        public string Result { get; set; } = string.Empty;
    }

    public enum RpaTaskState
    {
        Pending = 0,
        Running = 1,
        Completed = 2,
        Failed = 3
    }
}
