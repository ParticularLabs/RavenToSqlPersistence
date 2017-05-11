using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.ObjectBuilder;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;
using Raven.Client;
using Raven.Client.Document;

namespace RavenToSqlPersistence
{
    public class SubscriptionConverter
    {
        private ISubscriptionStorage persister;
        private DocumentStore docStore;

        public SubscriptionConverter(DocumentStore docStore, IBuilder builder)
        {
            this.docStore = docStore;
            this.persister = builder.Build<ISubscriptionStorage>();
        }

        public async Task Run()
        {
            
        }
    }
}
