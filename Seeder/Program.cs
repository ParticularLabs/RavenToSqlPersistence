using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using Raven.Client.Document;

namespace Seeder
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var docStore = new DocumentStore
            {
                Url = "http://localhost:8080",
                DefaultDatabase = "RavenToSqlPersistence",
            };

            var cfg = new EndpointConfiguration("RavenToSqlPersistence");
            cfg.EnableInstallers();
            cfg.SendFailedMessagesTo("error");

            var t = cfg.UseTransport<RabbitMQTransport>();
            t.ConnectionString("host=localhost");

            var p = cfg.UsePersistence<RavenDBPersistence>();

            p.SetDefaultDocumentStore(docStore);

            //cfg.SendOnly();

            var instance = await Endpoint.Start(cfg)
                .ConfigureAwait(false);

            var now = DateTime.UtcNow;

            var o = new SendOptions();
            o.DoNotDeliverBefore(now.Add(TimeSpan.FromHours(1)));
            o.RouteToThisEndpoint();
            await instance.Send(new MyMessage {At = now}, o)
                .ConfigureAwait(false);


            Console.ReadKey();

            await instance.Stop()
                .ConfigureAwait(false);
        }
    }
}


class MyMessage : IMessage
{
    public DateTime At { get; set; }
}

class MYHandler : IHandleMessages<MyMessage>
{
    public Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        var age = DateTime.UtcNow - DateTimeExtensions.ToUtcDateTime(context.MessageHeaders[Headers.TimeSent]);
        return Console.Out.WriteLineAsync($"{age} {context.MessageId}");
    }
}