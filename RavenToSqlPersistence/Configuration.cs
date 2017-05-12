using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using Raven.Client.Document;


static class Configuration
{
    public const string SubscriptionDocumentsStartWith = "Subscriptions/";
    public const string TimeoutDocumentsStartWith = "TimeoutDatas/";

    public static IEnumerable<SagaConversion> SagaConversions
    {
        get
        {
            yield return new SagaConversion("SagaDatas/", typeof(CreateRavenStuff.SagaData), "OrderId");
        }
    }

    public static DocumentStore ConfigureRavenDb()
    {
        var docStore = new DocumentStore()
        {
            Url = "http://localhost:8084",
            DefaultDatabase = "RavenToSqlPersistence",

        };

        // Do not Initialize DocumentStore
        return docStore;
    }

    public static void ConfigureSqlPersistence(EndpointConfiguration endpointConfiguration)
    {
        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        var connectionStr = @"Data Source=.\SQLEXPRESS;Initial Catalog=RavenToSqlPersistence;Integrated Security=True";
        persistence.SqlVariant(SqlVariant.MsSqlServer);
        persistence.ConnectionBuilder(
            connectionBuilder: () =>
            {
                return new SqlConnection(connectionStr);
            });
        persistence.SubscriptionSettings().DisableCache();
    }
}