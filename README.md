# RavenToSqlPersistence
Conversion utility for NServiceBus data from [RavenDB Persistence](https://docs.particular.net/persistence/ravendb/) to [SQL Persistence](https://docs.particular.net/persistence/sql/).

_**NOTE: This tool is not a push-button conversion. The code must be modified to match settings in your unique environment. Additionally, SQL Persistence supports only NServiceBus 6 and greater.**_

## How it works

The method of conversion varies depending upon persistence type.

### Subscriptions & Timeouts

* The tool queries the RavenDB database directly for every subscription and timeout document using the `LoadStartingWith` API.
* A "fake" NServiceBus endpoint is created using a fake message transport that does nothing, but real timeout/subscription storage configured to use [SQL Persistence](https://docs.particular.net/persistence/sql/).
* Subscription/timeout data is written to the SQL database _exactly_ how a normal NServiceBus endpoint would.

### Sagas

* Saga data handling is dependent upon actual class definitions of sagas and saga data, including specific type, namespace, and assembly information, which aren't easily replicated in a conversion tool.
* Because both RavenDB and SQL persistences use serialized JSON to store data, the conversion tool lifts the raw JSON data out of RavenDB, minimally modifies it, and persists it to a SQL table pre-created by an NServiceBus endpoint running SQL Persistence.

### Outbox

Conversion of Outbox data is not supported because:

1. Outbox data is very transient.
2. Most users would be converting from an environment where DTC transactions were enabled and the Outbox was not used anyway.

## Endpoint Conversion Process

Unless all endpoint data is stored in one centralized RavenDB database, endpoints can be converted one at a time. The following process should be attempted and verified in lower environments before being attempted on a production system.

1. Convert endpoint code to use SQL Persistence with your desired SQL dialect (Microsoft SQL Server, MySQL, or Oracle).
1. Execute the SQL scripts created by SQL Persistence to create the database schema in the target database.
1. Modify the conversion tool's [Configuration.cs](RavenToSqlPersistence/Configuration.cs) file with system-specific settings and connection strings. (See [Configuration](#configuration) below.)
1. Run the tool
1. Manually inspect the results in SQL tables to ensure everything converted as expected.
1. Attempt to run the endpoint using the new persistence.

## Configuration

All configuration is entered in code in the [Configuration.cs file](RavenToSqlPersistence/Configuration.cs).

### A note about document prefixes

Document ID prefixes can be different in each environment, due to backwards compatibility between many versions of RavenDB Persistence, as well as the possibility that a RavenDB `DocumentStore` can have a modified document ID convention by setting `documentStore.Conventions.FindTypeTagName` to a custom `Func<Type, string>` implementation.

The tool relies upon an accurate document prefix when using the `LoadStartingWith()` API to load all documents for a conversion.

The only safe way to determine what a document prefix for a specific conversion is:

1. Log into RavenDB Studio.
1. Open the database for the endpoint.
1. Navigate to **All Documents** and then to the document type for the specific conversion, such as Timeouts, Subscriptions, or a type of saga.
1. For all the subscription documents shown, determine the common prefix for all the document IDs, up to and including the `/` character. Some examples:
    * `Subscriptions/`
    * `TimeoutDatas/`
    * `MySagaDatas/`

### `SubscriptionDocumentsStartWith`

The common document prefix for all subscription documents. See [A note about document prefixes](#a-note-about-document-prefixes) above.

### `TimeoutDocumentsStartWith`

The common document prefix for all timeout documents. See [A note about document prefixes](#a-note-about-document-prefixes) above.

### `SagaConversions`

An `IEnumerable<SagaConversion>` that provides the tool information necessary to convert each type of Saga. Each Saga type handled by the endpoint must have its own `yield return new SagaConversion { ... }` containing information for that saga:

#### `documentPrefix`

The common document prefixes for all sagas of this type. See [A note about document prefixes](#a-note-about-document-prefixes) above.

#### `correlationId`

The name of the saga data property that is matched to data in incoming messages. It's easiest to identify this value from the saga code.

When configured to use RavenDB Persistence, look at the `ConfigureHowToFindSaga()` method:

```
// Endpoint using RavenDB Persistence with Saga<T>
protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MySagaData> mapper)
{
    mapper.ConfigureMapping<OrderPlaced>(message => message.PlacedOrderId).ToSaga(sagaData => sagaData.OrderId);
    mapper.ConfigureMapping<OrderBilled>(message => message.BilledOrderId).ToSaga(sagaData => sagaData.OrderId);
}
```

In this case the property identified by the `.ToSaga()` method is the correlationId, in this case, `OrderId`.

When already configured to use SQL Persistence, the CorrelationId already must be identified as a string property:

```
// Endpoint using SQL Persistence with SqlSaga<T>
protected override string CorrelationPropertyName => nameof(SagaData.OrderId);
// OR
protected override string CorrelationPropertyName => "OrderId";
```

In both of these cases, `OrderId` is the correlationId.

#### `tableName`

Once the endpoint is configured to use SQL Persistence, building the endpoint will create SQL scripts to create saga tables along with the other build output in the project's **bin** directory. The `tableName` parameter must be the table created by the endpoint to store that data.

* For Microsoft SQL and MySQL, this usually fits the format `{EndpointName}_{SagaName}`,
* For Oracle, this usually fits the format `{SagaName.ToUpper()`.

### `ConfigureRavenDb()`

This is where a DocumentStore must be configured to connect to your RavenDB database, similar to when [providing an external shared store at initialization](https://docs.particular.net/persistence/ravendb/connection#external-shared-store-at-initialization) of an NServiceBus endpoint.

*WARNING: Do not initialize the `DocumentStore` within the `ConfigureRavenDb()` method. The tool will initialize it for you.*

### `DestinationSqlType`

This configures what dialect of SQL you will convert to:

* `SqlVariant.MsSqlServer`
* `SqlVariant.MySQL`
* `SqlVariant.Oracle`

### `CreateSqlConnection`

Provides a `Func<DbConnection>` that instantiates a database connection to the chosen provider for use by SQL Persistence. This is passed to SQL Persistence's `ConnectionBuilder` method. See [SQL Persistence - Usage](https://docs.particular.net/persistence/sql/#usage) for examples of how to connect for each database variant.

## Questions?

Contact [support@particular.net](mailto:support@particular.net) for support.
