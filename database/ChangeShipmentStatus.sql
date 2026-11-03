-- Reemplaza ChangeShipmentStatus: ahora libera InUse del vehículo y del conductor al entregar o cancelar el envío.

CREATE OR ALTER PROCEDURE [dbo].[ChangeShipmentStatus]
    @Code     NVARCHAR(12),
    @IdStatus INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF dbo.IsValidShipmentStatusId(@IdStatus) = 0
        THROW 50040, 'Invalid status ID.', 1;

    DECLARE @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();
    DECLARE @CurrentName NVARCHAR(25), @NewName NVARCHAR(25);
    DECLARE @IdVehicle INT, @IdDriver INT;

    BEGIN TRANSACTION;

    SELECT @CurrentName = ss.Name, @IdVehicle = s.IdVehicle, @IdDriver = s.IdDriver
    FROM dbo.Shipments s WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN dbo.ShipmentStatuses ss ON ss.Id = s.IdStatus
    WHERE s.Code = @Code;

    IF @CurrentName IS NULL
        THROW 50041, 'Shipment not found.', 1;

    SELECT @NewName = Name FROM dbo.ShipmentStatuses WHERE Id = @IdStatus;

    -- Cambios permitidos: Pending -> On way | Canceled, On way -> Delivered | Canceled
    IF NOT (   (@CurrentName = 'Pending' AND @NewName IN ('On way', 'Canceled'))
            OR (@CurrentName = 'On way'  AND @NewName IN ('Delivered', 'Canceled')))
        THROW 50042, 'Invalid status change for this shipment.', 1;

    IF @NewName = 'On way' AND (@IdVehicle IS NULL OR @IdDriver IS NULL)
        THROW 50043, 'A shipment needs a vehicle and a driver before it can be on its way.', 1;

    UPDATE dbo.Shipments
    SET IdStatus    = @IdStatus,
        PickedUpAt  = CASE WHEN @NewName = 'On way'    THEN @Now ELSE PickedUpAt END,
        DeliveredAt = CASE WHEN @NewName = 'Delivered' THEN @Now ELSE DeliveredAt END
    WHERE Code = @Code;

    IF @NewName = 'On way'
        INSERT INTO dbo.ShipmentOnWayDates (ShipmentCode, [Date]) VALUES (@Code, @Now);
    ELSE IF @NewName = 'Delivered'
        INSERT INTO dbo.ShipmentDeliveredDates (ShipmentCode, [Date]) VALUES (@Code, @Now);
    ELSE IF @NewName = 'Canceled'
        INSERT INTO dbo.ShipmentCanceledDates (ShipmentCode, [Date]) VALUES (@Code, @Now);

    -- El vehículo y el conductor se ocupan al crear el envío (InUse = 1);
    -- se liberan cuando el envío termina, ya sea entregado o cancelado.
    IF @NewName IN ('Delivered', 'Canceled')
    BEGIN
        UPDATE dbo.Vehicles SET InUse = 0 WHERE Id = @IdVehicle;
        UPDATE dbo.Drivers  SET InUse = 0 WHERE Id = @IdDriver;
    END;

    COMMIT TRANSACTION;
END;

GO
