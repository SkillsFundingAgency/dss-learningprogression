using Microsoft.Azure.Documents;

namespace NCS.DSS.LearningProgression.CosmosDocumentClient
{
    public interface ICosmosDocumentClient
    {
        IDocumentClient GetDocumentClient();
    }
}