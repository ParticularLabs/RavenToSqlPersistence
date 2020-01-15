using System.Threading.Tasks;
using NServiceBus.Extensibility;
using Raven.Client.Document;
using NServiceBus.TimeoutPersisters.RavenDB;
using RavenToSqlPersistence.Utility;
using NServiceBus.Raw;
using NServiceBus;
using NServiceBus.Transport;
using NServiceBus.Routing;
using NServiceBus.DelayedDelivery;
using System;
using System.Collections.Concurrent;

static class TimeoutConverter
{
    public static async Task ConvertTimeouts(DocumentStore docStore)
    {
        var senderConfig = RawEndpointConfiguration.CreateSendOnly("DUMMY");
        var t = senderConfig.UseTransport<RabbitMQTransport>();
        t.ConnectionString("host=localhost");
        t.UseConventionalRoutingTopology();
        var sender = await RawEndpoint.Start(senderConfig);

        // Based on https://github.com/Particular/NServiceBus/blob/ead779f33e8bdec6844ee99a892729cfd7b7f0bc/src/NServiceBus.Core/DelayedDelivery/TimeoutManager/StoreTimeoutBehavior.cs#L63-L65
        foreach (var timeout in docStore.AllDocumentsStartingWith<TimeoutData>(Configuration.TimeoutDocumentsStartWith, 1024))
        {
            await Console.Out.WriteLineAsync($"Id:{timeout.Id} Time:{timeout.Time:s} Destination:{timeout.Destination} {timeout.OwningTimeoutManager}").ConfigureAwait(false);

            var body = timeout.State;
            var headers = timeout.Headers;
            var timestamp = timeout.Time;
            var destination = timeout.Destination;

            var request = new OutgoingMessage(
                messageId: timeout.Id,
                headers: headers,
                body: body
                );

            var operation = new TransportOperation(
                request,
                new UnicastAddressTag(destination)
                );
            
            operation.DeliveryConstraints.Add(new DoNotDeliverBefore(timestamp));

            await sender.Dispatch(
                outgoingMessages: new TransportOperations(operation),
                transaction: new TransportTransaction(),
                context: new ContextBag()
                )
                .ConfigureAwait(false);

        }
    }
}