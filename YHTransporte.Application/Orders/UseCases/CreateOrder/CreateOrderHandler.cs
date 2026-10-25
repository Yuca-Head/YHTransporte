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

        var addresses = await _addressRepo.GetByKeysAsync([command.OriginId, command.DestinationId], cancellationToken);

        var origin = addresses.FirstOrDefault(x => x.Key == command.OriginId);
        var destination = addresses.FirstOrDefault(x => x.Key == command.DestinationId);

        if(origin is null)
            return new AddressNotFound(command.OriginId);
        if(destination is null)
            return new AddressNotFound(command.DestinationId);
        
        var thirdParties = await _thirdPartyRepo.GetByKeysAsync([command.CustomerId, command.SupplierId], cancellationToken);
        

        ThirdParty? customer = thirdParties.FirstOrDefault(x => x.Key == command.CustomerId);
        ThirdParty? supplier = thirdParties.FirstOrDefault(x => x.Key == command.SupplierId);

        if(customer is null || customer?.Customer is null)
            return new CustomerNotFound(command.CustomerId);
        if(supplier is null || supplier?.Supplier is null)
            return new SupplierNotFound(command.SupplierId);
        
        var thirdParyAddresses = (await _addressRepo.GetAddressesFromThirdParties([supplier.Key, customer.Key], cancellationToken))
        .ToList();


        var result = thirdParyAddresses.Select(x =>
        {
            if(x.ThirdPartyId == supplier.Key && !x.Addresses.Any(y => y.Key == origin.Key))
                return new NotFound<string>($"El proveedor {supplier.Name} no contiene la dirección: {destination.Details} en {destination.Municipality}");
            
            if(x.ThirdPartyId == customer.Key && !x.Addresses.Any(y => y.Key == destination.Key))
                return new NotFound<string>($"El cliente {customer.Name} no contiene la dirección: {destination.Details} en {destination.Municipality}");

            return new(null!);
        });
        
        if(result.Any(x => x != default || x.Argument != null))
            return result.First(x => x != default || x.Argument != null);
        

        var order = new Order(supplier.Supplier, customer.Customer, origin, destination)
        {Description = command.Description!};

        await _repository.AddAsync(order, cancellationToken);
        return new Success();
    }
}   