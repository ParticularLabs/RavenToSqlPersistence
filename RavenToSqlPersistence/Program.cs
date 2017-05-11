using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Features;
using NServiceBus.ObjectBuilder;
using NServiceBus.Persistence.Sql;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;
using Raven.Client.Document;

namespace RavenToSqlPersistence
{
    class Program
    {
        static void Main(string[] args)
        {
            string endpointName = "RavenToSqlPersistence";
            try
            {
                RunAsync(endpointName).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("Conversion tool complete. Press Enter to exit.");
                Console.ReadLine();
            }
        }

        static async Task RunAsync(string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.UseTransport<MsmqTransport>();
            endpointConfiguration.SendOnly();
            endpointConfiguration.SendFailedMessagesTo("error");

            //var recoverability = endpointConfiguration.Recoverability();
            //recoverability.Immediate(customize => customize.NumberOfRetries(0));
            //recoverability.Delayed(customize => customize.NumberOfRetries(0));

            Configuration.SqlPersistence(endpointConfiguration);

            var docStore = Configuration.RavenDB(endpointName);
            docStore.Initialize();

            var endpoint = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            var builder = GetBuilderFeature.Builder;

            await new SubscriptionConverter(endpointName, docStore, builder).Run();
        }
    }

    public class GetBuilderFeature : Feature
    {
        public static IBuilder Builder { get; private set; }
        public GetBuilderFeature()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.RegisterStartupTask(builder => new Startup(builder));
        }

        class Startup : FeatureStartupTask
        {
            private IBuilder builder;

            public Startup(IBuilder builder)
            {
                GetBuilderFeature.Builder = builder;
            }

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
}
