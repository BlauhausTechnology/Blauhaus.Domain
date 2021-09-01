using Blauhaus.Domain.Client.Sync.Collection;

namespace Blauhaus.Domain.Tests.TestObjects.Client
{
    public class TestBaseListItem : BaseListItem<TestModel>, ITestListItem
    { 
        public string Name { get; set; }        
        
        protected override bool Update(TestModel model)
        {

            Name = model.Name;

            return true;
        }
    }

    public interface ITestListItem : IListItem<TestModel>
    {
        public string Name { get; set; }    
    }
}