using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync
{
    public class SyncUpdate<TEntity> where TEntity : IClientEntity
    {
        public SyncUpdate(TEntity current, bool canLoadMore = true)
        {
            Model = current;
            CanLoadMore = canLoadMore;
        }

        public TEntity Model { get; }
        public bool CanLoadMore { get; }
    }
}