using System;
using System.Collections.Generic;
using System.Data.Common;
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
            yield return new SagaConversion(
                documentPrefix: "SagaDatas/",
                correlationId: "OrderId",
                tableName: "RavenToSqlPersistence_MyFakeSaga");
        }
    }

    public static DocumentStore ConfigureRavenDb()
    {
        var docStore = new DocumentStore()
        {
            Url = "http://localhost:8080",
            DefaultDatabase = "RavenToSqlPersistence",

        };

        // Do not Initialize DocumentStore
        return docStore;
    }


    public static Func<DbConnection> CreateSqlConnection = () =>
    {
        return new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=RavenToSqlPersistence;Integrated Security=True");
    };
}