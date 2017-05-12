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
            try
            {
                RunAsync().GetAwaiter().GetResult();
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

        static async Task RunAsync()
        {
            var docStore = DbConfig.ConfigureRavenDB();
            docStore.Initialize();

            await SubscriptionConverter.ConvertSubscriptions(docStore);
            await TimeoutConverter.ConvertTimeouts(docStore);
        }
    }
}
