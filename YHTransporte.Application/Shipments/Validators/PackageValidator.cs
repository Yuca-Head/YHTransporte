using OneOf;
using YHTransporte.Application.Orders.Repositories;
using YHTransporte.Application.Shared.Results;
using YHTransporte.Application.Shipments.Repositories;
using YHTransporte.Application.ThirdParties.Repositories;
using YHTransporte.Core.Entities;
using YHTransporte.Core.Messages;

namespace YHTransporte.Application.Shipments.Validators;

/// <summary>
/// Reglas para crear o modificar un paquete:
/// el envío existe y aún no ha partido, la orden no está terminada, el producto es del proveedor de la orden,
/// y peso/cantidad son válidos.
/// </summary>
internal static class PackageValidator
{
    internal static async Task<OneOf<(Shipment Shipment, Product Product), OneOf.Types.NotFound, ValidationError>>
    Validate(IShipmentRepository shipmentRepository, IOrderRepository orderRepository, IThirdPartyRepository thirdPartyRepository,
    string shipmentCode, int productId, decimal weight, int amount, CancellationToken cancellationToken = default)
    {
        // Datos primero: no hace falta ir a la base de datos si ya están mal.
        List<string> errors = [];

        if(weight <= 0)
            errors.Add(ValidationErrors.MustBePositive("Paquete", "Peso"));
        else if(weight > Package.MaxWeight)
            errors.Add(ValidationErrors.GreaterThanAllowed("Paquete", "Peso", Package.MaxWeight));
        else if(weight != decimal.Round(weight, 2))
            errors.Add("El peso del paquete solo admite dos decimales.");

        if(amount <= 0)
            errors.Add(ValidationErrors.MustBePositive("Paquete", "Cantidad de producto"));

        if(errors.Count != 0)
            return new ValidationError(nameof(Package), errors);

        if(string.IsNullOrWhiteSpace(shipmentCode))
            return new OneOf.Types.NotFound();

        var shipment = (await shipmentRepository.GetByKeysAsync([shipmentCode], cancellationToken)).FirstOrDefault();

        if(shipment is null)
            return new OneOf.Types.NotFound();

        var order = (await orderRepository.GetByKeysAsync([shipment.OrderKey], cancellationToken)).FirstOrDefault();

        if(order is null)
            return new OneOf.Types.NotFound();

        if(ShipmentValidator.ValidateIsEditable(shipment.Status, order.Status) is ValidationError notEditable)
            return notEditable;

        var product = await thirdPartyRepository.GetSupplierProducts(order.SupplierId, cancellationToken)
        .FirstOrDefaultAsync(x => x.Key == productId, cancellationToken);

        if(product is null)
            return new ValidationError(nameof(Package.Product), DomainErrors.ProductDoesNotBelongToSupplier);

        return (shipment, product);
    }
}
