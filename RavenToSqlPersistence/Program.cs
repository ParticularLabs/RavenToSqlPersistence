using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Logging;


namespace RavenToSqlPersistence
{
    static class Program
    {
        static void Main(string[] args)
        {
            LogManager.Use<DefaultFactory>().Level(LogLevel.Warn);

            try
            {
                RunAsync().GetAwaiter().GetResult();
                Console.WriteLine("Conversion tool complete. Press Enter to exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error has occurred:");
                Console.WriteLine(e);
                Console.WriteLine();
                Console.WriteLine("Press Enter to exit.");
            }
            finally
            {
                Console.ReadLine();
            }
        }

        static async Task RunAsync()
        {
            CheckConfiguration();

            var docStore = Configuration.ConfigureRavenDb();
            docStore.Initialize();

            await SubscriptionConverter.ConvertSubscriptions(docStore);
            await TimeoutConverter.ConvertTimeouts(docStore);
            await SagaConverter.ConvertSagas(docStore);

            await EndpointProxy.StopAll();
        }

        private static void CheckConfiguration()
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Configuration.SagaConversions.ToList();
        }
    }
}
