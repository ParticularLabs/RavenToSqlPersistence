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
            var paging = new RavenPagingInformation();

            do
            {
                using (var session = docStore.OpenSession())
                {
                    var batch = session.Advanced.LoadStartingWith<T>(startingWith,
                        pagingInformation: paging, 
                        start: paging.NextPageStart, 
                        pageSize: pageSize);

                    foreach (var item in batch)
                    {
                        yield return item;
                    }
                }
            } while (!paging.IsLastPage());
        }
    }
}
