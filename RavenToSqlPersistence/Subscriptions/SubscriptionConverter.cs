using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.ObjectBuilder;
using NServiceBus.RavenDB.Persistence.SubscriptionStorage;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;
using Raven.Client;
using Raven.Client.Document;

static class SubscriptionConverter
{
    public static async Task ConvertSubscriptions(DocumentStore docStore)
    {
        var paging = new RavenPagingInformation();
        int pageSize = 1024;

        do
        {
            using (var session = docStore.OpenAsyncSession())
            {
                var batch = await session.Advanced.LoadStartingWithAsync<Subscription>(Configuration.SubscriptionDocumentsStartWith,
                    pagingInformation: paging, start: paging.NextPageStart, pageSize: pageSize);

                await ConvertBatch(batch);
            }
        } while (!paging.IsLastPage());
    }

    private static async Task ConvertBatch(IEnumerable<Subscription> batch)
    {
        foreach (var sub in batch)
        {
            Console.WriteLine($"Converting subscriptions for {sub.MessageType.TypeName}");
            foreach (var client in sub.Subscribers)
            {
                var newSubscriber = new Subscriber(client.TransportAddress, client.Endpoint);
                var proxy = EndpointProxy.GetProxy(client.Endpoint);
                await proxy.SubscriptionStorage.Subscribe(newSubscriber, sub.MessageType, new ContextBag());
            }
        }
    }
}
