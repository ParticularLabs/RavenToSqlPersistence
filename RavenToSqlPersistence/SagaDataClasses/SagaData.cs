using NServiceBus;

namespace CreateRavenStuff
{
    public class SagaData : ContainSagaData
    {
        public string OrderId { get; set; }
    }
}
