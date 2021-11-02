using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests.Base;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests
{
    public class GetWhereAsyncTests : BaseInMemoryDtoCacheTest
    {

        [Test]
        public async Task SHOULD_return_matching()
        {
            //Arrange
            Sut.Cache.Add(DtoOne.Id, DtoOne);
            Sut.Cache.Add(DtoTwo.Id, DtoTwo);
            Sut.Cache.Add(DtoThree.Id, DtoThree);
            
            //Act
            var result = await Sut.GetWhereAsync(x => x.Name == DtoTwo.Name || x.Name == DtoThree.Name);
            
            //Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.FirstOrDefault(x => x == DtoTwo), Is.Not.Null);
            Assert.That(result.FirstOrDefault(x => x == DtoThree), Is.Not.Null);
        }
         
    }
}