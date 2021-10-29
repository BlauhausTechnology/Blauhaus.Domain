using System.Threading.Tasks;

namespace Blauhaus.Domain.Abstractions.DtoHandlers
{
    public interface IModelOwner<TModel>
    {
        Task<TModel> GetModelAsync();
    }
}