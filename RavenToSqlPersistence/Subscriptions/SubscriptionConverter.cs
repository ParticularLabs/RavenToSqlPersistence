using System;
using System.Threading.Tasks;
using NServiceBus.Extensibility;
using NServiceBus.RavenDB.Persistence.SubscriptionStorage;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;
using Raven.Client.Document;
using RavenToSqlPersistence.Utility;

static class SubscriptionConverter
{
    public static async Task ConvertSubscriptions(DocumentStore docStore)
    {
        foreach (var subscription in docStore.AllDocumentsStartingWith<Subscription>(
            Configuration.SubscriptionDocumentsStartWith, pageSize: 1024))
        {
            await ConvertSubscription(subscription);
        }
    }

    private static async Task ConvertSubscription(Subscription subscription)
    {
        Console.WriteLine($"Converting subscriptions for {subscription.MessageType.TypeName}");
        foreach (var client in subscription.Subscribers)
        {
            var newSubscriber = new Subscriber(client.TransportAddress, client.Endpoint);
            var proxy = EndpointProxy.GetProxy(client.Endpoint);
            await proxy.SubscriptionStorage.Subscribe(newSubscriber, subscription.MessageType, new ContextBag());
        }
    }
}
