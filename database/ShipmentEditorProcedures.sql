-- ============================================================
-- Editor de envíos y paquetes
-- ============================================================
-- Códigos de error usados aquí:
--   50041 Envío no encontrado (mismo número que ChangeShipmentStatus)
--   50044 Solo se modifican envíos pendientes
--   50045 La orden ya está terminada o cancelada
--   50046 / 50047 Vehículo no existe / ya está en uso
--   50048 / 50049 Conductor no existe / ya está en uso
--   50090 - 50096 Paquetes (ver cada procedimiento)


-- ---------- Modificar envío ----------
-- Cambia vehículo, conductor, fecha estimada de recogida y descripción de un envío PENDIENTE.
-- Libera (InUse = 0) el vehículo/conductor que se quita y ocupa (InUse = 1) el que se pone,
-- todo en una sola transacción y bloqueando las filas para que dos envíos no tomen el mismo.

CREATE OR ALTER PROCEDURE [dbo].[UpdateShipment]
    @Code              NVARCHAR(12),
    @IdVehicle         INT                = NULL,
    @IdDriver          INT                = NULL,
    @EstimatedPickupAt DATETIMEOFFSET(7)  = NULL,
    @Description       NVARCHAR(120)      = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Description = NULLIF(LTRIM(RTRIM(@Description)), '');

    DECLARE @StatusName NVARCHAR(25), @OrderStatusName NVARCHAR(25);
    DECLARE @OldVehicle INT, @OldDriver INT;

    BEGIN TRANSACTION;

    SELECT @StatusName = ss.Name, @OrderStatusName = os.Name,
           @OldVehicle = s.IdVehicle, @OldDriver = s.IdDriver
    FROM dbo.Shipments s WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN dbo.ShipmentStatuses ss ON ss.Id = s.IdStatus
    INNER JOIN dbo.Orders o            ON o.Code = s.CodeOrder
    INNER JOIN dbo.OrderStatuses os    ON os.Id = o.IdStatus
    WHERE s.Code = @Code;

    IF @StatusName IS NULL
        THROW 50041, 'Shipment not found.', 1;

    IF @StatusName <> 'Pending'
        THROW 50044, 'Only pending shipments can be modified.', 1;

    IF @OrderStatusName IN ('Completed', 'Canceled')
        THROW 50045, 'The order is already completed or canceled.', 1;

    DECLARE @VehicleChanged BIT = CASE WHEN ISNULL(@IdVehicle, -1) <> ISNULL(@OldVehicle, -1) THEN 1 ELSE 0 END;
    DECLARE @DriverChanged  BIT = CASE WHEN ISNULL(@IdDriver, -1)  <> ISNULL(@OldDriver, -1)  THEN 1 ELSE 0 END;

    -- El nuevo vehículo debe existir y estar libre
    IF @VehicleChanged = 1 AND @IdVehicle IS NOT NULL
    BEGIN
        DECLARE @VehicleInUse BIT = (SELECT InUse FROM dbo.Vehicles WITH (UPDLOCK, HOLDLOCK) WHERE Id = @IdVehicle);

        IF @VehicleInUse IS NULL
            THROW 50046, 'Vehicle not found.', 1;
        IF @VehicleInUse = 1
            THROW 50047, 'The vehicle is already in use.', 1;
    END;

    -- El nuevo conductor debe existir y estar libre
    IF @DriverChanged = 1 AND @IdDriver IS NOT NULL
    BEGIN
        DECLARE @DriverInUse BIT = (SELECT InUse FROM dbo.Drivers WITH (UPDLOCK, HOLDLOCK) WHERE Id = @IdDriver);

        IF @DriverInUse IS NULL
            THROW 50048, 'Driver not found.', 1;
        IF @DriverInUse = 1
            THROW 50049, 'The driver is already in use.', 1;
    END;

    UPDATE dbo.Shipments
    SET IdVehicle         = @IdVehicle,
        IdDriver          = @IdDriver,
        EstimatedPickupAt = @EstimatedPickupAt,
        Description       = @Description
    WHERE Code = @Code;

    IF @VehicleChanged = 1
    BEGIN
        UPDATE dbo.Vehicles SET InUse = 0 WHERE Id = @OldVehicle;   -- no hace nada si era NULL
        UPDATE dbo.Vehicles SET InUse = 1 WHERE Id = @IdVehicle;
    END;

    IF @DriverChanged = 1
    BEGIN
        UPDATE dbo.Drivers SET InUse = 0 WHERE Id = @OldDriver;
        UPDATE dbo.Drivers SET InUse = 1 WHERE Id = @IdDriver;
    END;

    COMMIT TRANSACTION;
END;

GO


-- ---------- Guardar paquete (reemplaza la versión de MainProceduresDefinitions.sql) ----------
-- Igual que antes calcula el Id del paquete, pero ahora valida:
--   peso y cantidad > 0, que el envío exista y siga pendiente, que la orden no esté terminada
--   y que el producto sea del proveedor de la orden.

CREATE OR ALTER PROCEDURE [dbo].[InsertShipmentsDetail]
    @Code    NVARCHAR(12),
    @Product INT,
    @Kg      DECIMAL(10,2),
    @Amount  INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Kg IS NULL OR @Kg <= 0
        THROW 50091, 'The package weight must be greater than zero.', 1;

    IF @Amount IS NULL OR @Amount <= 0
        THROW 50092, 'The product amount must be greater than zero.', 1;

    BEGIN TRANSACTION;

    DECLARE @StatusName NVARCHAR(25), @OrderStatusName NVARCHAR(25), @SupplierId INT;

    SELECT @StatusName = ss.Name, @OrderStatusName = os.Name, @SupplierId = o.SupplierId
    FROM dbo.Shipments s WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN dbo.ShipmentStatuses ss ON ss.Id = s.IdStatus
    INNER JOIN dbo.Orders o            ON o.Code = s.CodeOrder
    INNER JOIN dbo.OrderStatuses os    ON os.Id = o.IdStatus
    WHERE s.Code = @Code;

    IF @StatusName IS NULL
        THROW 50090, 'Shipment not found.', 1;

    IF @StatusName <> 'Pending'
        THROW 50093, 'Packages can only be added or modified while the shipment is pending.', 1;

    IF @OrderStatusName IN ('Completed', 'Canceled')
        THROW 50094, 'The order is already completed or canceled.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.SupplierProducts WHERE IdSupplier = @SupplierId AND IdProduct = @Product)
        THROW 50095, 'The product does not belong to the supplier of the order.', 1;

    DECLARE @NextId INT;

    SELECT @NextId = ISNULL(MAX(Id), 0) + 1
    FROM dbo.ShipmentsDetails WITH (UPDLOCK, HOLDLOCK)
    WHERE Code_Shipment = @Code;

    INSERT INTO dbo.ShipmentsDetails (Code_Shipment, Id, Weight_kg, IdProduct, ProductAmount)
    VALUES (@Code, @NextId, @Kg, @Product, @Amount);

    COMMIT TRANSACTION;
END;

GO


-- ---------- Modificar paquete ----------
-- Mismas condiciones que para crearlo.

CREATE OR ALTER PROCEDURE [dbo].[UpdateShipmentsDetail]
    @Code    NVARCHAR(12),
    @Id      INT,
    @Product INT,
    @Kg      DECIMAL(10,2),
    @Amount  INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Kg IS NULL OR @Kg <= 0
        THROW 50091, 'The package weight must be greater than zero.', 1;

    IF @Amount IS NULL OR @Amount <= 0
        THROW 50092, 'The product amount must be greater than zero.', 1;

    BEGIN TRANSACTION;

    DECLARE @StatusName NVARCHAR(25), @OrderStatusName NVARCHAR(25), @SupplierId INT;

    SELECT @StatusName = ss.Name, @OrderStatusName = os.Name, @SupplierId = o.SupplierId
    FROM dbo.Shipments s WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN dbo.ShipmentStatuses ss ON ss.Id = s.IdStatus
    INNER JOIN dbo.Orders o            ON o.Code = s.CodeOrder
    INNER JOIN dbo.OrderStatuses os    ON os.Id = o.IdStatus
    WHERE s.Code = @Code;

    IF @StatusName IS NULL
        THROW 50090, 'Shipment not found.', 1;

    IF @StatusName <> 'Pending'
        THROW 50093, 'Packages can only be added or modified while the shipment is pending.', 1;

    IF @OrderStatusName IN ('Completed', 'Canceled')
        THROW 50094, 'The order is already completed or canceled.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.SupplierProducts WHERE IdSupplier = @SupplierId AND IdProduct = @Product)
        THROW 50095, 'The product does not belong to the supplier of the order.', 1;

    UPDATE dbo.ShipmentsDetails
    SET IdProduct     = @Product,
        Weight_kg     = @Kg,
        ProductAmount = @Amount
    WHERE Code_Shipment = @Code AND Id = @Id;

    IF @@ROWCOUNT = 0
        THROW 50096, 'Package not found.', 1;

    COMMIT TRANSACTION;
END;

GO
