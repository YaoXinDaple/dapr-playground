using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;

namespace Rpa.Client
{
    internal class ControllerActor : Actor, IController, IRemindable
    {
        private readonly string taskIdKeys = "Task-ids";
        public ControllerActor(ActorHost host) : base(host)
        {
        }

        public async Task<string[]> ListRegisteredTaskIdsAsync()
        {
            return await StateManager.GetStateAsync<string[]>(taskIdKeys) ?? Array.Empty<string>();
        }

        public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            throw new NotImplementedException();
        }

        public async Task RegisterTaskIdsAsync(string[] taskIds)
        {
            await StateManager.SetStateAsync<string[]>(taskIdKeys, taskIds);
        }

        public async Task ReceivedNewInvoiceTaskRequest(Guid requestId)
        {
            var taskIds = await StateManager.GetStateAsync<string[]>(taskIdKeys);

            string requestIdString = requestId.ToString();
            if (!taskIds.Contains(requestIdString))
            {
                var actorId = new ActorId(requestIdString);
                var actor = ActorProxy.Create<IController>(actorId, "Rpa.Client");
                await actor.RegisterTaskIdsAsync([.. taskIds, requestIdString]);
            }
        }
    }
}
