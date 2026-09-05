using YHTransporte.Application.Addresses.Repositories;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Shared.Results;

namespace YHTransporte.Application.Addresses.UseCases.CreateAddress;

public sealed class CreateAddressHandler(IAddressRepository repository)
{
    private readonly IAddressRepository _repository = repository ?? 
    throw new ArgumentException(nameof(repository));


}