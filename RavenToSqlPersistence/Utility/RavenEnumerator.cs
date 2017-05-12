using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.RavenDB.Persistence.SubscriptionStorage;
using Raven.Abstractions.Data;
using Raven.Client.Document;
using Raven.Client;

namespace RavenToSqlPersistence.Utility
{
    public static class RavenEnumerator
    {
        public static IEnumerable<T> AllDocumentsStartingWith<T>(this DocumentStore docStore, string startingWith, int pageSize)
        {
            using (var session = (DocumentSession)docStore.OpenSession())
            {
                foreach (var doc in AllDocumentsStartingWith(docStore, typeof(T), startingWith, pageSize))
                {
                    object entity = session.ConvertToEntity(typeof(T), doc.Key, doc.DataAsJson, doc.Metadata);
                    yield return (T)entity;
                }
            }
        }

        public static IEnumerable<JsonDocument> AllDocumentsStartingWith(this DocumentStore docStore, Type documentType, string startingWith, int pageSize)
        {
            var paging = new RavenPagingInformation();

            {
                do
                {
                    var documents = docStore.DatabaseCommands.StartsWith(startingWith, null, paging.NextPageStart, pageSize, paging);
                    foreach (var doc in documents)
                    {
                        yield return doc;
                    }
                } while (!paging.IsLastPage());
            }
        }
    }
}
