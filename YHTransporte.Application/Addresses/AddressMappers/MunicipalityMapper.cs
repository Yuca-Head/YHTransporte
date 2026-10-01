using YHTransporte.Application.Abstractions;
using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Addresses.AddressMappers;  

public class MunicipalityMapper : IMapper<MunicipalityDto, Municipality>
{
    
    public static MunicipalityDto ToValue(Municipality municipality)
    => new(municipality.Key, municipality.Name, DepartmentMapper.ToValue(municipality.Department));

    public static Municipality ToEntity(MunicipalityDto dto)
    => new(dto.Name, DepartmentMapper.ToEntity(dto.Department)) {Key = dto.Id};


}