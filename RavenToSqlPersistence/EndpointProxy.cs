using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Features;
using NServiceBus.ObjectBuilder;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;
using RavenToSqlPersistence;

class EndpointProxy
{
    private readonly string endpointName;
    private IEndpointInstance endpoint;
    private IBuilder builder;

    private static Dictionary<string, EndpointProxy> proxyDict =
        new Dictionary<string, EndpointProxy>(StringComparer.InvariantCultureIgnoreCase);

    private EndpointProxy(string endpointName)
    {
        this.endpointName = endpointName;
    }

    public static EndpointProxy GetProxy(string endpointName)
    {
        EndpointProxy proxy = null;
        if (!proxyDict.TryGetValue(endpointName, out proxy))
        {
            proxy = new EndpointProxy(endpointName);
            proxyDict.Add(endpointName, proxy);
            proxy.Initialize();
        }
        return proxy;
    }

    private void Initialize()
    {
        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
        var endpointConfiguration = new EndpointConfiguration(endpointName);
        endpointConfiguration.UseTransport<MsmqTransport>();
        endpointConfiguration.SendOnly();
        endpointConfiguration.SendFailedMessagesTo("error");

        var builderHolder = new BuilderHolder();
        var settings = endpointConfiguration.GetSettings();
        settings.Set<BuilderHolder>(builderHolder);
        
        DbConfig.ConfigureSqlPersistence(endpointConfiguration);

        this.endpoint = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        this.builder = builderHolder.Builder;
    }

    public ISubscriptionStorage SubscriptionStorage => builder.Build<ISubscriptionStorage>();
}