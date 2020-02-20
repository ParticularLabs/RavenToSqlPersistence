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
        var ignoreMachineName = true; // Must be true for RabbitMQ

        var senderConfig = RawEndpointConfiguration.CreateSendOnly("DUMMY");
        var t = senderConfig.UseTransport<RabbitMQTransport>();
        t.ConnectionString("host=localhost");
        t.UseConventionalRoutingTopology();
        var sender = await RawEndpoint.Start(senderConfig)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;

        // Based on https://github.com/Particular/NServiceBus/blob/ead779f33e8bdec6844ee99a892729cfd7b7f0bc/src/NServiceBus.Core/DelayedDelivery/TimeoutManager/StoreTimeoutBehavior.cs#L63-L65
        foreach (var timeout in docStore.AllDocumentsStartingWith<TimeoutData>(Configuration.TimeoutDocumentsStartWith, 1024))
        {
            await Console.Out.WriteLineAsync($"Id:{timeout.Id} Time:{timeout.Time:s} Destination:{timeout.Destination} {timeout.OwningTimeoutManager}").ConfigureAwait(false);

            var body = timeout.State;
            var headers = timeout.Headers;
            var timestamp = timeout.Time;
            var destination = timeout.Destination;
            var id = headers["NServiceBus.MessageId"];

            if (ignoreMachineName)
            {
                var at = destination.LastIndexOf("@", StringComparison.InvariantCulture);

                if (at != -1)
                {
                    destination = destination.Substring(0, at);
                }
            }

            var age = now - timestamp;

            var request = new OutgoingMessage(
                messageId: timeout.Id,
                headers: headers,
                body: body
                );


            var operation = new TransportOperation(
                request,
                new UnicastAddressTag(destination)
                );

            if (age.Ticks>0)
            {
                await Console.Out.WriteLineAsync($"Warning: Message {id} was scheduled for {timestamp} which passed {age} ago.")
                    .ConfigureAwait(false);
            }
            else
            {
                operation.DeliveryConstraints.Add(new DoNotDeliverBefore(timestamp));
            }

            await sender.Dispatch(
                outgoingMessages: new TransportOperations(operation),
                transaction: new TransportTransaction(),
                context: new ContextBag()
                )
                .ConfigureAwait(false);

        }
    }
}
