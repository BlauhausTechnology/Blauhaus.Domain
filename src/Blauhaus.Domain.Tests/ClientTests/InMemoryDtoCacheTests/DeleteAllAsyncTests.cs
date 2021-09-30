using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests.Base;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests
{
    public class DeleteAllAsyncTests : BaseInMemoryDtoCacheTest
    {

        [Test]
        public async Task SHOULD_delete_all()
        {
            //Arrange
            Sut.Cache.Add(DtoThree.Id, DtoThree);
            Sut.Cache.Add(DtoTwo.Id, DtoTwo);
            
            //Act
            await Sut.DeleteAllAsync();
            
            //Assert
            Assert.That((await Sut.GetAllAsync()).Count, Is.EqualTo(0));
        }
         
         
    }
}