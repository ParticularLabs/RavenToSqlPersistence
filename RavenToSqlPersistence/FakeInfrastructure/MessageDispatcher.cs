using System.Threading.Tasks;
using NServiceBus.Extensibility;
using NServiceBus.Transport;

namespace RavenToSqlPersistence
{
    public class MessageDispatcher : IDispatchMessages
    {
        public Task Dispatch(TransportOperations outgoingMessages, TransportTransaction transaction, ContextBag context)
        {
            return Task.CompletedTask;
        }
    }
}