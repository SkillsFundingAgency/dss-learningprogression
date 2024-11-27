﻿using Microsoft.Azure.Cosmos;

namespace NCS.DSS.LearningProgression.Cosmos.Containers
{
    public class CustomerContainer : ICustomerContainer
    {
        private readonly Container _container;

        public CustomerContainer(Container container)
        {
            _container = container;
        }

        public Container GetContainer() => _container;
    }
}
