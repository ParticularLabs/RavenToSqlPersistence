using System.Threading.Tasks;
using NServiceBus.Extensibility;
using Raven.Client.Document;
using NServiceBus.TimeoutPersisters.RavenDB;
using RavenToSqlPersistence.Utility;

static class TimeoutConverter
{
    public static async Task ConvertTimeouts(DocumentStore docStore)
    {
        foreach (var timeout in docStore.AllDocumentsStartingWith<TimeoutData>(Configuration.TimeoutDocumentsStartWith, 1024))
        {
            var proxy = EndpointProxy.GetProxy(timeout.OwningTimeoutManager);
            await proxy.TimeoutStorage.Add(timeout.ToCoreTimeoutData(), new ContextBag());
        }
    }
}