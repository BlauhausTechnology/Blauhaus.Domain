using Blauhaus.Domain.Common.Entities;

namespace Blauhaus.Domain.Client.Sync
{
    public class SyncUpdate<TEntity> where TEntity : IClientEntity
    {
        public SyncUpdate(TEntity current, bool canLoadMore = true)
        {
            Current = current;
            CanLoadMore = canLoadMore;
        }

        public TEntity Current { get; }
        public bool CanLoadMore { get; }
    }
}