using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Persistence;
using NServiceBus.Persistence.Sql;
using Raven.Client;
using Raven.Client.Document;
using RavenToSqlPersistence.Utility;
using NServiceBus.Persistence.Sql.ScriptBuilder;
using NServiceBus.Sagas;
using Raven.Json.Linq;

static class SagaConverter
{
    private static readonly Version PersistenceVersion;

    static SagaConverter()
    {
        var assembly = typeof(SqlPersistence).Assembly;
        var version = FileVersionInfo.GetVersionInfo(assembly.Location);
        PersistenceVersion = new Version(
            major: version.FileMajorPart,
            minor: version.FileMinorPart,
            build: version.FileBuildPart,
            revision: version.FilePrivatePart);
    }

    public static async Task ConvertSagas(DocumentStore docStore)
    {
        foreach (var conversion in Configuration.SagaConversions)
        {
            await ConvertSagaType(docStore, conversion);
        }
    }

    private static async Task ConvertSagaType(DocumentStore docStore, SagaConversion conversion)
    {
        foreach (var sagaDocument in docStore.AllDocumentsStartingWith(conversion.SagaDataType, conversion.DocumentPrefix, 1024))
        {
            var data = sagaDocument.DataAsJson;
            var idString = sagaDocument.Key.Substring(conversion.DocumentPrefix.Length);
            var sagaId = new Guid(idString);

            RavenJObject metadata = new RavenJObject();
            metadata["OriginalMessageId"] = data["OriginalMessageId"];
            metadata["Originator"] = data["Originator"];
            var metadataText = metadata.ToString();

            data.Remove("OriginalMessageId");
            data.Remove("Originator");
            var dataText = data.ToString();

            var correlationValue = data[conversion.CorrelationId].Value<string>();

            using (var connection = Configuration.CreateSqlConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = conversion.GetSagaInsertCommandText();
                    AddParameter(cmd, "Id", sagaId);
                    AddParameter(cmd, "Metadata", metadataText);
                    AddParameter(cmd, "Data", dataText);
                    AddParameter(cmd, "PersistenceVersion", PersistenceVersion.ToString());
                    AddParameter(cmd, "SagaTypeVersion", new Version(0,0,0,0).ToString());
                    AddParameter(cmd, "CorrelationId", correlationValue);
                    await cmd.ExecuteNonQueryAsync();
                }
                
            }
        }
    }

    private static void AddParameter(DbCommand command, string paramName, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = paramName;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}