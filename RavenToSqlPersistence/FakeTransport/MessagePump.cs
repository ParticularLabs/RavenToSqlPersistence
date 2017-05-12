using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Transport;

namespace RavenToSqlPersistence
{
    public class MessagePump : IPushMessages
    {
        public Task Init(Func<MessageContext, Task> onMessage, Func<ErrorContext, Task<ErrorHandleResult>> onError, CriticalError criticalError, PushSettings settings)
        {
            return Task.CompletedTask;
        }

        public void Start(PushRuntimeSettings limitations)
        {
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }
    }
}