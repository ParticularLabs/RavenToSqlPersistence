using System;
using System.Reflection;
using NServiceBus;

public class SagaConversion
{
    public string DocumentPrefix { get; private set; }
    public Type SagaDataType { get; private set; }
    public string CorrelationId { get; private set; }
    public string EndpointName { get; private set; }
    public string SagaClassName { get; private set; }
    public string TableName { get; private set; } = "RavenToSqlPersistence_MyFakeSaga"; // TODO: Temporary Value

    private readonly PropertyInfo correlationProperty;

    public SagaConversion(string documentPrefix, Type sagaDataType, string correlationId, string endpointName, string sagaClassName)
    {
        if (!documentPrefix.EndsWith("/"))
        {
            throw new ArgumentException($"documentPrefix for saga type '{sagaDataType.FullName} must end with '/' character.");
        }

        if(!typeof(IContainSagaData).IsAssignableFrom(sagaDataType))
        {
            throw new ArgumentException($"Saga data type {sagaDataType} does not inherit ContainSagaData or implement IContainSagaData.");
        }

        correlationProperty = sagaDataType.GetProperty(correlationId);
        if (correlationProperty == null)
        {
            throw new ArgumentException($"Could not find correlation property named '{correlationId}' on saga data type '{sagaDataType.FullName}'.");
        }

        // TODO: More verification of table prefix & class name

        DocumentPrefix = documentPrefix;
        SagaDataType = sagaDataType;
        CorrelationId = correlationId;
        EndpointName = endpointName;
        SagaClassName = sagaClassName;
    }

    public object GetCorrelationValue(IContainSagaData sagaData)
    {
        return correlationProperty.GetMethod.Invoke(sagaData, null);
    }

    public string GetSagaInsertCommandText()
    {
        return $@"
insert into {TableName}
(
    Id,
    Metadata,
    Data,
    PersistenceVersion,
    SagaTypeVersion,
    Concurrency,
    Correlation_{CorrelationId}
)
values
(
    @Id,
    @Metadata,
    @Data,
    @PersistenceVersion,
    @SagaTypeVersion,
    1,
    @CorrelationId
)";
    }
}
