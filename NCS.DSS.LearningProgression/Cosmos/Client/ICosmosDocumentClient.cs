using Microsoft.Azure.Documents;

namespace NCS.DSS.LearningProgression.Cosmos.Client
{
    public interface ICosmosDocumentClient
    {
        IDocumentClient GetDocumentClient();
    }
}