using Blauhaus.Domain.Client.Sync.Collection;

namespace Blauhaus.Domain.Tests.ClientTests.TestObjects
{
    public class TestListItem : ListItem<TestModel>
    { 
        public string Name { get; set; }        
        
        protected override void Update(TestModel model)
        {

            Name = model.Name;
        }
    }
}