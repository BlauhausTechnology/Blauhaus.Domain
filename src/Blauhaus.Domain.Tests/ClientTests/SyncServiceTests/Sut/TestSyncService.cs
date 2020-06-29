using System;
using System.Collections.Generic;
using System.Net.Http;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Domain.Client.Sync.Client;
using Blauhaus.Domain.Client.Sync.Service;
using Blauhaus.Domain.Tests.ClientTests.TestObjects;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncServiceTests.Sut
{
    public class TestSyncService : BaseSyncService<TestSyncCommand>
    {
        private readonly ISyncClient<TestModel, TestSyncCommand> _testSyncClient;
        private readonly ISyncClient<TestModelToo, TestSyncCommand> _testSyncClientToo;

        public TestSyncService(
            IAnalyticsService analyticsService,
            ISyncStatusHandlerFactory factory,
            ISyncClient<TestModel, TestSyncCommand> testSyncClient,
            ISyncClient<TestModelToo, TestSyncCommand> testSyncClientToo) 
                : base(analyticsService, factory)
        {
            _testSyncClient = testSyncClient;
            _testSyncClientToo = testSyncClientToo;
        }

        protected override IList<Func<TestSyncCommand, ClientSyncRequirement, ISyncStatusHandler, IObservable<object>>> GetSyncConnections()
        {
            return new List<Func<TestSyncCommand, ClientSyncRequirement, ISyncStatusHandler, IObservable<object>>>
            {
                (command, req, handler) => _testSyncClient.Connect(command, req, handler),
                (command, req, handler) => _testSyncClientToo.Connect(command, req, handler),
            };
        }
    }
}