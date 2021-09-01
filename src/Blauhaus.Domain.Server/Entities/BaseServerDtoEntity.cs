//using System;
//using System.Security.Cryptography;
//using Blauhaus.Domain.Abstractions.Entities;

//namespace Blauhaus.Domain.Server.Entities
//{
//    public abstract class BaseServerDtoEntity : BaseServerDtoEntity<Guid>
//    {
//        protected BaseServerDtoEntity()
//        {
//        }
//        protected BaseServerDtoEntity(
//            DateTime createdAt, Guid id, EntityState entityState, string serializedDto)
//            : base(createdAt, id, entityState, serializedDto)
//        {
//        }

//        protected override Guid GenerateId()
//        {
//            return Guid.NewGuid();
//        }
//    }

//    public abstract class BaseServerDtoEntity<TId> : BaseServerEntity<TId>, ISerializedDto
//    {
//        protected BaseServerDtoEntity()
//        {
//        }
         
//        protected BaseServerDtoEntity(
//            DateTime createdAt, TId id, EntityState entityState, string serializedDto)
//                : base(createdAt, id, entityState)
//        {
//            SerializedDto = serializedDto;
//        }

//        public string SerializedDto { get; private set; } = string.Empty;
//    }
//}