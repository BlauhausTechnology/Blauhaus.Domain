using Blauhaus.Domain.Client.Sync.Collection;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class TestViewElementUpdater : IListItemUpdater<TestModel, TestListItem>
    {
        public TestListItem Update(TestModel model, TestListItem element)
        {
            element.Name = model.Name;
            return element;
        }
    }
}