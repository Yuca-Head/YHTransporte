using System.Reflection.Metadata;
using OneOf;
using OneOf.Types;
using YHTransporte.Application.Addresses.Repositories;
using YHTransporte.Application.Addresses.Results;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Shared;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Application.ThirdParties.Results;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Orders.UseCases.CreateOrder;

public sealed class CreateOrderHandler(IOrderRepository repository, 
IAddressRepository addressRepo, IThirdPartyRepository thirdPartyRepo)
{

    private readonly IOrderRepository _repository = repository;
    private readonly IAddressRepository _addressRepo = addressRepo;
    private readonly IThirdPartyRepository _thirdPartyRepo = thirdPartyRepo;

    public async Task<OneOf<Success, AddressNotFound, CustomerNotFound, SupplierNotFound, NotFound<string>>>
    Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        if(! await _addressRepo.Exists(command.OriginId, cancellationToken))
            return new AddressNotFound(command.OriginId);
        if(! await _addressRepo.Exists(command.DestinationId, cancellationToken))
            return new AddressNotFound(command.DestinationId);
        
        var customerTask = _thirdPartyRepo.GetByKeysAsync([command.CustomerId], cancellationToken);
        var supplierTask = _thirdPartyRepo.GetByKeysAsync([command.SupplierId], cancellationToken);

        ThirdParty? customer = (await customerTask).FirstOrDefault();
        ThirdParty? supplier = (await supplierTask).FirstOrDefault();

        if(customer is null || customer?.Customer is null)
            return new CustomerNotFound(command.CustomerId);
        if(supplier is null || supplier?.Supplier is null)
            return new SupplierNotFound(command.SupplierId);

        

        var origin = new Address("-", new("-", new("-"))) {Key = command.OriginId};
        var destination = new Address("-", new("-", new("-"))) {Key = command.DestinationId};

        if(!customer.Addresses.Any(x => x.Key == destination.Key))
            return new NotFound<string>($"El cliente {customer.Name} no contiene la dirección: {destination.Details} en {destination.Municipality}");
        if(!supplier.Addresses.Any(x => x.Key == origin.Key))
            return new NotFound<string>($"El proveedor {supplier.Name} no contiene la dirección: {destination.Details} en {destination.Municipality}");
        

        var order = new Order(supplier.Supplier, customer.Customer, origin, destination){Description = command.Description!};

        await _repository.AddAsync(order, cancellationToken);
        return new Success();
    }
}   