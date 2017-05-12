using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Features;
using NServiceBus.ObjectBuilder;
using NServiceBus.Timeout.Core;
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

    public static Task StopAll()
    {
        var proxies = proxyDict.Values.ToList();
        proxyDict.Clear();
        var stopTasks = proxies.Select(p => p.Stop());
        return Task.WhenAll(stopTasks);
    }

    private void Initialize()
    {
        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
        var endpointConfiguration = new EndpointConfiguration(endpointName);
        endpointConfiguration.UseTransport<FakeTransport>();
        endpointConfiguration.SendFailedMessagesTo("error");

        var nsbAssembly = typeof(Endpoint).Assembly;
        endpointConfiguration.DisableFeature(nsbAssembly.GetType("NServiceBus.Features.ReceiveStatisticsPerformanceCounters"));
        endpointConfiguration.DisableFeature(nsbAssembly.GetType("NServiceBus.ReceiveStatisticsFeature"));

        var builderHolder = new BuilderHolder();
        var settings = endpointConfiguration.GetSettings();
        settings.Set<BuilderHolder>(builderHolder);
        
        Configuration.ConfigureSqlPersistence(endpointConfiguration);

        this.endpoint = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        this.builder = builderHolder.Builder;

        var testQuery = builder.Build<IQueryTimeouts>();
        var testPersist = builder.Build<IPersistTimeouts>();
    }

    public async Task Stop()
    {
        await endpoint.Stop();
    }

    public ISubscriptionStorage SubscriptionStorage => builder.Build<ISubscriptionStorage>();
    public IPersistTimeouts TimeoutStorage => builder.Build<IPersistTimeouts>();
}