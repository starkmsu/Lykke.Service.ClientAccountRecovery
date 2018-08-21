using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    [UsedImplicitly]
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LogTableEntity, RecoveryContext>();
            CreateMap<RecoveryContext, LogTableEntity>()
                .ForMember(m => m.RowKey, o => o.MapFrom(s => LogTableEntity.GetRowKey(s.SeqNo)))
                .ForMember(m => m.PartitionKey, o => o.MapFrom(s => LogTableEntity.GetPartitionKey(s.RecoveryId)));
        }
    }
}
