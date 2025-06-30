using Dapr.Actors;

namespace Rpa.Client
{
    public interface IController:IActor
    {
        Task RegisterTaskIdsAsync(string[] deviceIds);
        Task<string[]> ListRegisteredTaskIdsAsync();
        Task ReceivedNewInvoiceTaskRequest(Guid requestId);
    }

    public class ControllerData
    { 
        public string[] TaskIds { get; set; } = Array.Empty<string>();
    }
}
