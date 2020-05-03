using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Tests.ClientTests.SqliteTests._TestObjects
{
    public interface ITestModel : IClientEntity
    {
        string RootEntityName { get; }
    }
}