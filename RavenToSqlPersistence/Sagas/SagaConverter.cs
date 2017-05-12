using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Document;

static class SagaConverter
{
    public static async Task ConvertSagas(DocumentStore docStore)
    {
        foreach (var conversion in Configuration.SagaConversions)
        {
            ConvertSagaType(docStore, conversion);
        }
    }

    private static async Task ConvertSagaType(DocumentStore docStore, SagaConversion conversion)
    {

    }
}