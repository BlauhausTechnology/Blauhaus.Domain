using System;
using Blauhaus.Domain.Tests.Base;
using Blauhaus.Domain.Tests.TestObjects.Server;

namespace Blauhaus.Domain.Tests.ServerTests.MyServerDtoRepositoryTests.Base
{
    public abstract class BaseMyServerRepositoryDtoTest : BaseEfCoreTest<MyServerDtoLoader, MyDbContext>
    {

        protected MyServerEntityBuilder ExistingEntityBuilder  = null!;
        protected Guid ExistingEntityId = Guid.NewGuid();

        protected override void SetupDbContext(MyDbContext setupContext)
        {
            ExistingEntityBuilder = new MyServerEntityBuilder()
                .With(x => x.Id, ExistingEntityId);
        }

        protected override MyServerDtoLoader ConstructSut()
        {
            AdditionalSetup(context =>
            {
                context.Add(ExistingEntityBuilder.Object);
                context.SaveChanges();
            });

            return base.ConstructSut();
        }
    }
}