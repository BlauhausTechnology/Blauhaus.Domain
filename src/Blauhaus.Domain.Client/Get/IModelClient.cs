using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Blauhaus.Domain.Client.Get
{
    public interface IModelClient<TModel>
    {
        IObservable<TModel> Load(Guid id, CancellationToken token);
    }
}