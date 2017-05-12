using System.Threading.Tasks;
using CreateRavenStuff;
using NServiceBus;
using NServiceBus.Persistence.Sql;

namespace CreateRavenStuff
{
    public class SagaData : ContainSagaData
    {
        public string OrderId { get; set; }
    }
}

namespace My.Fake.Saga.Namespace
{
    public class MyFakeSaga : SqlSaga<CreateRavenStuff.SagaData>, IAmStartedByMessages<FakeMsg>
    {
        protected override void ConfigureMapping(IMessagePropertyMapper mapper)
        {
            mapper.ConfigureMapping<FakeMsg>(m => m.OrderId);
        }

        protected override string CorrelationPropertyName => "OrderId";
        public Task Handle(FakeMsg message, IMessageHandlerContext context)
        {
            return Task.CompletedTask;
        }
    }

    public class FakeMsg : ICommand
    {
        public string OrderId { get; set; }
    }
}
