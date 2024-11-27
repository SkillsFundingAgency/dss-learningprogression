using Microsoft.Azure.Cosmos;

namespace NCS.DSS.LearningProgression.Cosmos.Containers
{
    public interface ILearningProgressionContainer
    {
        Container GetContainer();
    }
}
