using System;
using System.Reflection;
using NServiceBus;

public class SagaConversion
{
    public string DocumentPrefix { get; private set; }
    public string CorrelationId { get; private set; }
    public string TableName { get; private set; }

    public SagaConversion(string documentPrefix, string correlationId, string tableName)
    {
        if (!documentPrefix.EndsWith("/"))
        {
            throw new ArgumentException($"documentPrefix '{documentPrefix} must end with '/' character.");
        }

        // TODO: More verification of table prefix & class name

        DocumentPrefix = documentPrefix;
        CorrelationId = correlationId;
        TableName = tableName;
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
