using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Analytics.TestHelpers;
using Blauhaus.DeviceServices.Abstractions.Connectivity;
using Blauhaus.DeviceServices.TestHelpers.MockBuilders;
using Blauhaus.Domain.TestHelpers.MockBuilders.Common;
using Blauhaus.Errors.Handler;
using Blauhaus.Ioc.Abstractions;
using Blauhaus.TestHelpers.BaseTests;
using Blauhaus.TestHelpers.MockBuilders;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests._Base
{
    public abstract class BaseDomainTest<TSut> : BaseServiceTest<TSut> where TSut : class
    {
        [SetUp]
        public virtual void Setup()
        {
            Cleanup();

            AddService(x => MockAnalyticsService.Object);
            AddService(x => MockErrorHandler.Object);
            AddService(x => MockConnectivityService.Object);
            AddService(x => MockServiceLocator.Object);
        }

        protected AnalyticsServiceMockBuilder MockAnalyticsService => AddMock<AnalyticsServiceMockBuilder, IAnalyticsService>().Invoke();
        protected ConnectivityServiceMockBuilder MockConnectivityService => AddMock<ConnectivityServiceMockBuilder, IConnectivityService>().Invoke();
        protected MockBuilder<IErrorHandler> MockErrorHandler => AddMock<IErrorHandler>().Invoke();
        protected ServiceLocatorMockBuilder MockServiceLocator => AddMock<ServiceLocatorMockBuilder, IServiceLocator>().Invoke();


    }
}