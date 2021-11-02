using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests.Base;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests
{
    public class GetIdsWhereAsyncTests : BaseInMemoryDtoCacheTest
    {

        [Test]
        public async Task IF_Dtos_exist_SHOULD_return_matching()
        {
            //Arrange
            Sut.Cache.Add(DtoOne.Id, DtoOne);
            Sut.Cache.Add(DtoTwo.Id, DtoTwo);
            Sut.Cache.Add(DtoThree.Id, DtoThree);
            
            //Act
            var result = await Sut.GetIdsWhereAsync(x => x.Name == DtoTwo.Name || x.Name == DtoOne.Name);
            
            //Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Contains(DtoTwo.Id));
            Assert.That(result.Contains(DtoOne.Id));
        }
         
    }
}