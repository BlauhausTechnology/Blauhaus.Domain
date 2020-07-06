using System.Collections.Generic;
using Blauhaus.Ioc.Abstractions;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Common
{
    public class ServiceLocatorMockBuilder : BaseMockBuilder<ServiceLocatorMockBuilder, IServiceLocator>
    {
        public ServiceLocatorMockBuilder Where_Resolve_returns<T>(T value)
        {
            Mock.Setup(x => x.Resolve<T>()).Returns(value);
            return this;
        }

        public ServiceLocatorMockBuilder Where_Resolve_returns_sequence<T>(List<T> values)
        {
            var queue = new Queue<T>(values);
            Mock.Setup(x => x.Resolve<T>()).Returns(queue.Dequeue);
            return this;
        }
    }
}