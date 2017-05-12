using NServiceBus.Settings;
using NServiceBus.Transport;

namespace RavenToSqlPersistence
{
    public class FakeTransport : TransportDefinition
    {
        public override TransportInfrastructure Initialize(SettingsHolder settings, string connectionString)
        {
            return new FakeTransportInfrastructure();
        }

        public override string ExampleConnectionStringForErrorMessage => null;

        public override bool RequiresConnectionString => false;
    }
}
