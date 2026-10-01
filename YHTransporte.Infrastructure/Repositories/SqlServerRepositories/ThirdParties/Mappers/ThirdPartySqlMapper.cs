using YHTransporte.Application.Abstractions;
using YHTransporte.Core.Entities;
using YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Dtos;

namespace YHTransporte.Infrastructure.Repositories.SqlServerRepositories.ThirdParties.Mappers;

public class ThirdPartySqlMapper : IMapper<ThirdPartySqlDto, ThirdParty>
{
    public static ThirdParty ToEntity(ThirdPartySqlDto value)
    {
        var entity = new ThirdParty(value.Name){Key = value.Id};

        if(value.IsCustomer)
            entity.BecomeCustomer();
        if(value.IsSupplier)
            entity.BecomeSupplier();
        
        return entity;
    } 

    public static ThirdPartySqlDto ToValue(ThirdParty entity)
    => new(entity.Key, entity.Name, entity.Supplier is not null, entity.Customer is not null);
}