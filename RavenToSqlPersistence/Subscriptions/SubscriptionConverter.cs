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

public class SubscriptionConverter
{
    public static async Task ConvertSubscriptions(DocumentStore docStore)
    {
        var list = new List<Subscription>();
        using (var session = docStore.OpenAsyncSession())
        {
            var batch = await session.Advanced.LoadStartingWithAsync<Subscription>("Subscriptions/", start: 0, pageSize: 1024);
            list.AddRange(batch);
        }

        foreach (var sub in list)
        {
            foreach (var client in sub.Subscribers)
            {
                var newSubscriber = new Subscriber(client.TransportAddress, client.Endpoint);
                var proxy = EndpointProxy.GetProxy(client.Endpoint);
                await proxy.SubscriptionStorage.Subscribe(newSubscriber, sub.MessageType, new ContextBag());
            }
        }
    }
}
