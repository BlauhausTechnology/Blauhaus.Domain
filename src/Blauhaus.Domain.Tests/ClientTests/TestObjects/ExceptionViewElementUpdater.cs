using System;
using Blauhaus.Domain.Client.Sync.Collection;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class ExceptionViewElementUpdater : IListItemUpdater<TestModel, TestListItem>
    {
        public TestListItem Update(TestModel model, TestListItem element)
        {
            throw new Exception("This is an exceptionally bad thing that just happened");
        }
    }
}