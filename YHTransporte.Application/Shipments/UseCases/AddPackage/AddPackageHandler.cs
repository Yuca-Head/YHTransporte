using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.Validators;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Shipments.UseCases.AddPackage;

public sealed class AddPackageHandler(IShipmentRepository repository, IOrderRepository orderRepository,
IThirdPartyRepository thirdPartyRepository)
{
    private readonly IShipmentRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    private readonly IThirdPartyRepository _thirdPartyRepository = thirdPartyRepository ?? throw new ArgumentNullException(nameof(thirdPartyRepository));

    /// <summary>
    /// Agrega un paquete a un envío que todavía no ha partido.
    /// </summary>
    public async Task<OneOf<Success, OneOf.Types.NotFound, ValidationError>>
    Handle(AddPackageCommand command, CancellationToken cancellationToken = default)
    {
        var result = await PackageValidator.Validate(_repository, _orderRepository, _thirdPartyRepository,
        command.ShipmentCode, command.ProductId, command.Weight, command.ProductAmount, cancellationToken);

        if(result.IsT1)
            return result.AsT1;
        if(result.IsT2)
            return result.AsT2;

        var (shipment, product) = result.AsT0;

        // El Id del paquete lo calcula el procedimiento InsertShipmentsDetail.
        var package = new Package(product, command.Weight, command.ProductAmount) { ShipmentCode = shipment.Key };

        await _repository.AddPackage(shipment.Key, package, cancellationToken);

        return new Success();
    }
}
