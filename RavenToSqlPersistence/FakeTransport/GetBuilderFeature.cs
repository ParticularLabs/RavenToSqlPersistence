using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.ObjectBuilder;

class BuilderHolder
{
    public IBuilder Builder { get; set; }
}

class GetBuilderFeature : Feature
{
    public GetBuilderFeature()
    {
        EnableByDefault();
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        var builderHolder = context.Settings.Get<BuilderHolder>();
        context.RegisterStartupTask(builder =>
        {
            builderHolder.Builder = builder;
            return new Noop();
        });
    }

    class Noop : FeatureStartupTask
    {
        protected override Task OnStart(IMessageSession session)
        {
            return Task.CompletedTask;
        }

        protected override Task OnStop(IMessageSession session)
        {
            return Task.CompletedTask;
        }
    }
}