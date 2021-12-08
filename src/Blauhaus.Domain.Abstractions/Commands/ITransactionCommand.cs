using System;

namespace Blauhaus.Domain.Abstractions.Commands
{
    public interface ITransactionCommand
    {
        public Guid TransactionId { get; set; }
    }
}