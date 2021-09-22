using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blauhaus.Domain.Abstractions.Entities;
using Blauhaus.Domain.Abstractions.Sync;
using Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncDtoCacheTests.Base;
using Blauhaus.Domain.Tests.TestObjects.Client;
using Blauhaus.Domain.Tests.TestObjects.Common;
using NUnit.Framework;

namespace Blauhaus.Domain.Tests.ClientTests.SyncTests.SyncDtoCacheTests
{
    public class SavedSyncedDtosAsyncTests : BaseSyncDtoCacheTest
    {
        private DtoBatch<MyDto, Guid> _dtoBatch = null!;

        public override void Setup()
        {
            base.Setup();

            _dtoBatch = DtoBatch<MyDto, Guid>.Create(new []{DtoOne, DtoTwo}, 1);
        }

        [Test]
        public async Task SHOULD_add_Dto_to_Cache()
        {
            //Act
            await Sut.SaveSyncedDtosAsync(_dtoBatch);
            
            //Assert
            Assert.That(await Sut.GetOneAsync(DtoOne.Id), Is.Not.Null);
            Assert.That(await Sut.GetOneAsync(DtoTwo.Id), Is.Not.Null);
        }

        [Test]
        public async Task SHOULD_populate_additional_properties()
        {
            //Act
            await Sut.SaveSyncedDtosAsync(_dtoBatch);
            
            //Assert
            Assert.That((await Sut.GetOneAsync(DtoOne.Id)).Name, Is.EqualTo(DtoOne.Name));
            Assert.That((await Sut.GetOneAsync(DtoTwo.Id)).Name, Is.EqualTo(DtoTwo.Name));
        }
         
        [Test]
        public async Task SHOULD_set_SyncState_to_InSync()
        {
            //Act
            await Sut.SaveSyncedDtosAsync(_dtoBatch);
            
            //Assert
            var entityOne = await Connection.Table<MySyncedDtoEntity>().FirstAsync(x => x.Id == DtoOne.Id);
            Assert.That(entityOne.SyncState, Is.EqualTo(SyncState.InSync));
            var entityTwo = await Connection.Table<MySyncedDtoEntity>().FirstAsync(x => x.Id == DtoTwo.Id);
            Assert.That(entityTwo.SyncState, Is.EqualTo(SyncState.InSync));
        }

        [Test]
        public async Task SHOULD_not_add_duplicate()
        {
            //Act
            await Sut.HandleAsync(DtoOne);
            await Sut.HandleAsync(DtoOne);
            
            //Assert
            var all = await Sut.GetAllAsync();
            Assert.That(all.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task SHOULD_update_subscribers_to_that_id()
        {
            //Arrange
            var publishedDtos = new List<MyDto>();
            await Sut.SubscribeAsync(d =>
            {
                publishedDtos.Add(d);
                return Task.CompletedTask;
            }, dto => dto.Id == DtoOne.Id);
            
            //Act
            await Sut.HandleAsync(DtoOne);
            
            //Assert
            Assert.That(publishedDtos.Count, Is.EqualTo(1));
            Assert.That(publishedDtos.First(), Is.EqualTo(DtoOne));
        }

        [Test]
        public async Task SHOULD_NOT_update_subscribers_to_different_id()
        {
            //Arrange
            var publishedDtos = new List<MyDto>();
            await Sut.SubscribeAsync(d =>
            {
                publishedDtos.Add(d);
                return Task.CompletedTask;
            }, dto => dto.Id == DtoOne.Id);
            
            //Act
            await Sut.HandleAsync(DtoThree);
            
            //Assert
            Assert.That(publishedDtos.Count, Is.EqualTo(0));
        }
         
    }
}