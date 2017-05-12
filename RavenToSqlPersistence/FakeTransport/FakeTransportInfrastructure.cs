using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Routing;
using NServiceBus.Transport;

namespace RavenToSqlPersistence
{
    public class FakeTransportInfrastructure : TransportInfrastructure
    {
        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure()
        {
            return new TransportReceiveInfrastructure(() => new MessagePump(), () => new QueueCreator(), StartupCheck);
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure()
        {
            return new TransportSendInfrastructure(() => new MessageDispatcher(), StartupCheck);
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure()
        {
            return new TransportSubscriptionInfrastructure(() => new SubscriptionManager());
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            return instance;
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            return logicalAddress.EndpointInstance.Endpoint;
        }

        Task<StartupCheckResult> StartupCheck()
        {
            return Task.FromResult(StartupCheckResult.Success);
        }

        public override IEnumerable<Type> DeliveryConstraints
        {
            get { yield break; }
        }

        public override TransportTransactionMode TransactionMode => TransportTransactionMode.ReceiveOnly;

        public override OutboundRoutingPolicy OutboundRoutingPolicy => new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Unicast, OutboundRoutingType.Unicast);
    }
}