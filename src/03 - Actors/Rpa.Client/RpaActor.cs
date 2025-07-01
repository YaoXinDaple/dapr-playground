using Dapr.Actors.Runtime;

namespace Rpa.Client
{
    public class RpaActor : Actor, IRpaWorker, IRemindable
    {
        public RpaActor(ActorHost host) : base(host)
        {
        }

        protected sealed override Task OnActivateAsync()
        {
            Console.WriteLine($"Actor {Id} activated at {DateTime.UtcNow}");
            return base.OnActivateAsync();
        }

        protected sealed override Task OnDeactivateAsync()
        {
            // Provides opportunity to perform optional cleanup.
            Console.WriteLine($"Deactivating actor id: {Id}");
            return Task.CompletedTask;
        }


        public async Task CreateAsync(Guid taskId)
        {
            Console.WriteLine($"Actor {Id} create task with ID: {taskId}");
            RpaWorkerData data = new RpaWorkerData
            {
                TaskId = taskId,
                TaskType = "InvoiceProcessing", // Example task type
                CreatedAt = DateTimeOffset.UtcNow,
                TaskState = RpaTaskState.Running
            };
            await StateManager.SetStateAsync<RpaWorkerData>(taskId.ToString(), data);
            int randomSec = Random.Shared.Next(10, 90); // Simulate some work

            await RegisterReminderAsync("taskShouldBeFailed", taskId.ToByteArray(), TimeSpan.FromSeconds(randomSec), TimeSpan.FromMilliseconds(-1));
        }

        public async Task StartAsync(Guid taskId)
        {
            Console.WriteLine($"Actor {Id} starting task with ID: {taskId}");
            RpaWorkerData data = await StateManager.GetStateAsync<RpaWorkerData>(taskId.ToString());
            data.StartedAt = DateTimeOffset.UtcNow;
            await StateManager.SetStateAsync<RpaWorkerData>(taskId.ToString(), data);
            int randomSec = Random.Shared.Next(10, 90); // Simulate some work
            await Task.Delay(randomSec * 1000); // Simulate some work

            await RegisterReminderAsync("taskCanBeCompleted", taskId.ToByteArray(), TimeSpan.FromSeconds(randomSec), TimeSpan.FromMilliseconds(-1));
        }

        public async Task StopAsync(Guid taskId)
        {
            RpaWorkerData data = await StateManager.GetStateAsync<RpaWorkerData>(taskId.ToString());
            if (data.TaskState == RpaTaskState.Completed)
            {
                throw new InvalidOperationException($"Task :{taskId} was Completed,can not stop a completed task");
            }
            if (data.TaskState == RpaTaskState.Failed)
            {
                throw new InvalidOperationException($"Task :{taskId} was Failed,can not stop a failed task");
            }
            data.TaskState = RpaTaskState.Failed;
            data.Result = "task has been stopped";
            data.CompletedAt = DateTimeOffset.UtcNow;
        }


        public async Task<string> GetCurrentTaskStateAsync(Guid taskId)
        {
            RpaWorkerData data = await StateManager.GetStateAsync<RpaWorkerData>(taskId.ToString());
            return data.TaskState.ToString();
        }

        private async Task CompleteAsync(Guid taskId)
        {
            RpaWorkerData data = await StateManager.GetStateAsync<RpaWorkerData>(taskId.ToString());
            if (data.TaskState == RpaTaskState.Running)
            {
                data.TaskState = RpaTaskState.Completed;
                data.CompletedAt = DateTimeOffset.UtcNow;
                data.Result = " Task Completed!";
            }
            else
            {
                throw new InvalidOperationException($"Can not complete a {data.TaskState.ToString()} task");
            }
        }

        private async Task FailAsync(Guid taskId)
        {
            RpaWorkerData data = await StateManager.GetStateAsync<RpaWorkerData>(taskId.ToString());

            data.TaskState = RpaTaskState.Failed;
            data.CompletedAt = DateTimeOffset.UtcNow;
            data.Result = " Task Failed!";

            await StateManager.SetStateAsync<RpaWorkerData>(taskId.ToString(), data);

            //if (data.TaskState == RpaTaskState.Running)
            //{
            //    data.TaskState = RpaTaskState.Failed;
            //    data.CompletedAt = DateTimeOffset.UtcNow;
            //    data.Result = " Task Failed!";
            //}
            //else
            //{
            //    throw new InvalidOperationException($"Can not fail a {data.TaskState.ToString()} task");
            //}
        }

        public async Task<string> GetResultAsync(Guid taskId)
        {
            RpaWorkerData data = await StateManager.GetStateAsync<RpaWorkerData>(taskId.ToString());
            return data.Result;
        }

        public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            Guid taskId = new Guid(state);  // 从 byte[] 还原 Guid
            Console.WriteLine($"接收到定时提醒，Name:{reminderName},State:{taskId}");

            if (reminderName == "taskCanBeCompleted")
            {
                return CompleteAsync(taskId);
            }
            else if (reminderName == "taskShouldBeFailed")
            {
                return FailAsync(taskId);
            }
            return Task.CompletedTask;
        }
    }
}
