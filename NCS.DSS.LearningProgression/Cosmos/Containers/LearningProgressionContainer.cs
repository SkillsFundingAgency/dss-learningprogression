using Microsoft.Azure.Cosmos;

namespace NCS.DSS.LearningProgression.Cosmos.Containers
{
    public class LearningProgressionContainer : ILearningProgressionContainer
    {
        private readonly Container _container;

        public LearningProgressionContainer(Container container)
        {
            _container = container;
        }

        public Container GetContainer() => _container;
    }
}
