using System;
using System.Threading.Tasks;
using NServiceBus.Logging;


namespace RavenToSqlPersistence
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.Use<DefaultFactory>().Level(LogLevel.Warn);

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
