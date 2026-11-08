using OneOf;
using OneOf.Types;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.Shipments.Validators;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Core.Entities;

namespace YHTransporte.Application.Shipments.UseCases.ModifyPackage;

public sealed class ModifyPackageHandler(IShipmentRepository repository, IOrderRepository orderRepository,
IThirdPartyRepository thirdPartyRepository)
{
    private readonly IShipmentRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    private readonly IThirdPartyRepository _thirdPartyRepository = thirdPartyRepository ?? throw new ArgumentNullException(nameof(thirdPartyRepository));

    /// <summary>
    /// Modifica producto, peso y cantidad de un paquete. Solo se permite con las mismas condiciones
    /// que para crearlo (el envío no ha partido y la orden no está terminada).
    /// </summary>
    public async Task<OneOf<Success, OneOf.Types.NotFound, ValidationError>>
    Handle(ModifyPackageCommand command, CancellationToken cancellationToken = default)
    {
        var result = await PackageValidator.Validate(_repository, _orderRepository, _thirdPartyRepository,
        command.ShipmentCode, command.ProductId, command.Weight, command.ProductAmount, cancellationToken);

        if(result.IsT1)
            return result.AsT1;
        if(result.IsT2)
            return result.AsT2;

        var (shipment, product) = result.AsT0;

        if(!shipment.Packages.Any(x => x.Key == command.PackageId))
            return new OneOf.Types.NotFound();

        var package = new Package(product, command.Weight, command.ProductAmount)
        {
            Key = command.PackageId,
            ShipmentCode = shipment.Key
        };

        await _repository.UpdatePackage(shipment.Key, package, cancellationToken);

        return new Success();
    }
}
