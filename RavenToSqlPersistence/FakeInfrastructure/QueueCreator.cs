using System.Threading.Tasks;
using NServiceBus.Transport;

namespace RavenToSqlPersistence
{
    public class QueueCreator : ICreateQueues
    {
        public Task CreateQueueIfNecessary(QueueBindings queueBindings, string identity)
        {
            return Task.CompletedTask;
        }
    }
}