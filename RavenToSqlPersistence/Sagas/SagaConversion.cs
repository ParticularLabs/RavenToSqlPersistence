using System;
using NServiceBus;

public class SagaConversion
{
    public string DocumentPrefix { get; private set; }
    public Type SagaDataType { get; private set; }
    public string CorrelationId { get; private set; }

    public SagaConversion(string documentPrefix, Type sagaDataType, string correlationId)
    {
        if (!documentPrefix.EndsWith("/"))
        {
            throw new ArgumentException($"documentPrefix for saga type '{sagaDataType.FullName} must end with '/' character.");
        }

        if(!typeof(IContainSagaData).IsAssignableFrom(sagaDataType))
        {
            throw new ArgumentException($"Saga data type {sagaDataType} does not inherit ContainSagaData or implement IContainSagaData.");
        }

        var correlationProperty = sagaDataType.GetProperty(correlationId);
        if (correlationProperty == null)
        {
            throw new ArgumentException($"Could not find correlation property named '{correlationId}' on saga data type '{sagaDataType.FullName}'.");
        }

        DocumentPrefix = documentPrefix;
        SagaDataType = sagaDataType;
        CorrelationId = correlationId;
    }
}
