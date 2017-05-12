using System;
using System.Threading.Tasks;
using NServiceBus.Extensibility;
using NServiceBus.Transport;

namespace RavenToSqlPersistence
{
    public class SubscriptionManager : IManageSubscriptions
    {
        public Task Subscribe(Type eventType, ContextBag context)
        {
            return Task.CompletedTask;
        }

        public Task Unsubscribe(Type eventType, ContextBag context)
        {
            return Task.CompletedTask;
        }
    }
}