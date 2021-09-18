using System.Threading.Tasks;
using Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests.Base;
using Blauhaus.Errors;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.InMemoryDtoCacheTests
{
    public class GetOneAsyncTests : BaseInMemoryDtoCacheTest
    {

        [Test]
        public async Task IF_Dto_exists_SHOULD_return()
        {
            //Arrange
            Sut.Cache.Add(DtoThree.Id, DtoThree);
            
            //Act
            var result = await Sut.GetOneAsync(DtoThree.Id);
            
            //Assert
            Assert.That(result, Is.EqualTo(DtoThree));
        }
        
        [Test]
        public async Task IF_Dto_does_not_existS_SHOULD_return_throw()
        {
            //Arrange
            Sut.Cache.Add(DtoThree.Id, DtoThree);
            
            //Act
            Assert.ThrowsAsync<ErrorException>(async ()=> await Sut.GetOneAsync(DtoOne.Id));
             
        }
         
    }
}