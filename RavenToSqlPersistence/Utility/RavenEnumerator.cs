using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus.RavenDB.Persistence.SubscriptionStorage;
using Raven.Client.Document;
using Raven.Client;

namespace RavenToSqlPersistence.Utility
{
    public static class RavenEnumerator
    {
        public static IEnumerable<T> AllDocumentsStartingWith<T>(this DocumentStore docStore, string startingWith, int pageSize)
        {
            foreach (var entity in AllDocumentsStartingWith(docStore, typeof(T), startingWith, pageSize))
            {
                yield return (T) entity;
            }
        }

        public static IEnumerable<object> AllDocumentsStartingWith(this DocumentStore docStore, Type documentType, string startingWith, int pageSize)
        {

            var paging = new RavenPagingInformation();
            using (var session = (DocumentSession)docStore.OpenSession())
            {
                do
                {
                    var documents = docStore.DatabaseCommands.StartsWith(startingWith, null, paging.NextPageStart, pageSize, paging);
                    foreach (var doc in documents)
                    {
                        object entity = session.ConvertToEntity(documentType, doc.Key, doc.DataAsJson, doc.Metadata);
                        yield return entity;
                    }
                } while (!paging.IsLastPage());
            }
        }
    }
}
