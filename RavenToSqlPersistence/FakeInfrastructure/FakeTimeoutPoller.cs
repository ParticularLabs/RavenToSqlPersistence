using System;
using System.Threading.Tasks;
using NServiceBus.Features;
using NServiceBus.Timeout.Core;

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