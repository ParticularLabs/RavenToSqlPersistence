using System.Threading.Tasks;
using NServiceBus.Extensibility;
using Raven.Client.Document;
using NServiceBus.TimeoutPersisters.RavenDB;

static class TimeoutConverter
{
    public static async Task ConvertTimeouts(DocumentStore docStore)
    {
        using (var session = docStore.OpenAsyncSession())
        {
            var batch = await session.Advanced.LoadStartingWithAsync<TimeoutData>(
                    keyPrefix: Configuration.TimeoutDocumentsStartWith, 
                    start: 0, 
                    pageSize: 1024);

            foreach (var timeout in batch)
            {
                var proxy = EndpointProxy.GetProxy(timeout.OwningTimeoutManager);
                await proxy.TimeoutStorage.Add(timeout.ToCoreTimeoutData(), new ContextBag());
            }
        }
    }
}