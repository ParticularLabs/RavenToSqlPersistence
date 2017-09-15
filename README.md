# RavenToSqlPersistence
Conversion utility for NServiceBus data from [RavenDB Persistence](https://docs.particular.net/persistence/ravendb/) to [SQL Persistence](https://docs.particular.net/persistence/sql/).

_**NOTE: This tool is not a push-button conversion. The code must be modified to match settings in your unique environment.**_

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
1. Modify the conversion tool's [Configuration.cs](RavenToSqlPersistence/Configuration.cs) file with system-specific settings and connection strings. (See [Configuration](#Configuration) below.)
1. Run the tool
1. Manually inspect the results in SQL tables to ensure everything converted as expected.
1. Attempt to run the endpoint using the new persistence.

## Configuration

stub
