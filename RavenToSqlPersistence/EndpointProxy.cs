using System;
using System.Collections.Generic;
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
        
        DbConfig.ConfigureSqlPersistence(endpointConfiguration);

        this.endpoint = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        this.builder = builderHolder.Builder;

        var testQuery = builder.Build<IQueryTimeouts>();
        var testPersist = builder.Build<IPersistTimeouts>();
    }

    public ISubscriptionStorage SubscriptionStorage => builder.Build<ISubscriptionStorage>();
    public IPersistTimeouts TimeoutStorage => builder.Build<IPersistTimeouts>();
}

public class FakeTimeoutPoller : IQueryTimeouts
{
    public Task<TimeoutsChunk> GetNextChunk(DateTime startSlice)
    {
        var emptyChunk = new TimeoutsChunk(new TimeoutsChunk.Timeout[0], DateTime.UtcNow.AddYears(10));
        return Task.FromResult(emptyChunk);
    }
}

public class SwapTimeoutQueryFeature : Feature
{
    public SwapTimeoutQueryFeature()
    {
        EnableByDefault();
        this.DependsOn("SqlTimeoutFeature");
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        context.Container.RegisterSingleton(typeof(IQueryTimeouts), new FakeTimeoutPoller());
    }
}