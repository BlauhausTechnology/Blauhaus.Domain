using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Analytics.TestHelpers.MockBuilders;
using Blauhaus.DeviceServices.Abstractions.Connectivity;
using Blauhaus.DeviceServices.TestHelpers.MockBuilders;
using Blauhaus.Errors.Handler;
using Blauhaus.Ioc.Abstractions;
using Blauhaus.Ioc.TestHelpers;
using Blauhaus.TestHelpers.BaseTests;
using Blauhaus.TestHelpers.MockBuilders;
using Blauhaus.Time.Abstractions;
using Blauhaus.Time.TestHelpers.MockBuilders;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.Base
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
            AddService(x => MockTimeService.Object);
        }

        protected AnalyticsServiceMockBuilder MockAnalyticsService => AddMock<AnalyticsServiceMockBuilder, IAnalyticsService>().Invoke();
        protected ConnectivityServiceMockBuilder MockConnectivityService => AddMock<ConnectivityServiceMockBuilder, IConnectivityService>().Invoke();
        protected MockBuilder<IErrorHandler> MockErrorHandler => AddMock<IErrorHandler>().Invoke();
        protected ServiceLocatorMockBuilder MockServiceLocator => AddMock<ServiceLocatorMockBuilder, IServiceLocator>().Invoke();
        protected TimeServiceMockBuilder MockTimeService => AddMock<TimeServiceMockBuilder, ITimeService>().Invoke();


    }
}