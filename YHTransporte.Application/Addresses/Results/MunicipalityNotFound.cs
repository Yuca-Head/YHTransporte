using YHTransporte.Application.Addresses.Dto;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Addresses.Results;

public sealed record MunicipalityNotFound(IEnumerable<int> Ids) : NotFound(Ids);