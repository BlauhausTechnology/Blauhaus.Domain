using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.TestHelpers.MockBuilders;

namespace Blauhaus.Domain.TestHelpers.MockBuilders.Client.SyncService
{
    public class SyncServiceMockBuilder : BaseMockBuilder<SyncServiceMockBuilder, ISyncService>
    {

        public SyncServiceMockBuilder()
        {
            Where_Sync_returns(new List<SyncUpdate>());
        }

        public SyncServiceMockBuilder Where_Sync_returns(IEnumerable<SyncUpdate> values)
        {
            Mock.Setup(x => x.Sync()).Returns(Observable.Create<SyncUpdate>(observer =>
            {
                foreach (var syncUpdate in values)
                {
                    observer.OnNext(syncUpdate);
                }
                
                observer.OnCompleted();

                return Disposable.Empty;
            }));
            return this;
        }

        public SyncServiceMockBuilder Where_Sync_returns(SyncUpdate value)
        {
            Mock.Setup(x => x.Sync()).Returns(Observable.Create<SyncUpdate>(observer =>
            {
                observer.OnNext(value);
                observer.OnCompleted();
                return Disposable.Empty;
            }));
            return this;
        }
        
        public SyncServiceMockBuilder Where_Sync_completes()
        {
            Mock.Setup(x => x.Sync()).Returns(Observable.Create<SyncUpdate>(observer =>
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }));
            return this;
        }

        public SyncServiceMockBuilder Where_Sync_fails(Exception e)
        {
            Mock.Setup(x => x.Sync()).Returns(Observable.Create<SyncUpdate>(observer =>
            {
                observer.OnError(e);
                return Disposable.Empty;
            }));
            return this;
        }
    }
}