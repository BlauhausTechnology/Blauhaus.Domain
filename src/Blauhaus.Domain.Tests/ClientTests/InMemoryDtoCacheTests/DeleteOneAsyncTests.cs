using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests.Base;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests
{
    public class DeleteOneAsyncTests : BaseInMemoryDtoCacheTest
    {

        [Test]
        public async Task IF_Dto_exists_SHOULD_delete()
        {
            //Arrange
            Sut.Cache.Add(DtoThree.Id, DtoThree);
            
            //Act
            await Sut.DeleteOneAsync(DtoThree.Id);
            
            //Assert
            Assert.That((await Sut.TryGetOneAsync(DtoThree.Id)), Is.Null);
        }
         
         
    }
}