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
    private readonly string endpointName;
    private readonly DocumentStore docStore;
    private readonly ISubscriptionStorage persister;

    public SubscriptionConverter(string endpointName, DocumentStore docStore, IBuilder builder)
    {
        this.endpointName = endpointName;
        this.docStore = docStore;
        this.persister = builder.Build<ISubscriptionStorage>();
    }

    public async Task Run()
    {
        var list = new List<Subscription>();
        using (var session = docStore.OpenAsyncSession())
        {
            var batch = await session.Advanced.LoadStartingWithAsync<Subscription>("Subscriptions/", start: 0, pageSize: 1024);
            list.AddRange(batch);
        }

        var mine = list
            .Where(sub => sub.Subscribers.Any(EndpointMatches))
            .ToList();

        foreach (var sub in mine)
        {
            foreach (var client in sub.Subscribers.Where(EndpointMatches))
            {
                var newSubscriber = new Subscriber(client.TransportAddress, client.Endpoint);
                await persister.Subscribe(newSubscriber, sub.MessageType, new ContextBag());
            }
        }
    }

    private bool EndpointMatches(SubscriptionClient client)
    {
        return string.Equals(endpointName, client.Endpoint, StringComparison.InvariantCultureIgnoreCase);
    }
}
