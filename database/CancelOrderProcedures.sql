-- ---------- Cancelar órdenes ----------
-- Los dos cancelan la orden y sus envíos en una sola transacción:
--   CancelOrderSafe   -> cancela los envíos pendientes; si algún envío va en camino, no cancela nada (error 50054).
--   CancelOrderUnsafe -> cancela todos los envíos que no estén entregados, también los que van en camino.
-- Los envíos ya entregados no se tocan. Cada envío cancelado libera su vehículo y su conductor (ChangeShipmentStatus).

CREATE OR ALTER PROCEDURE [dbo].[CancelOrderSafe]
    @Code NVARCHAR(12)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM dbo.Orders WITH (UPDLOCK, HOLDLOCK) WHERE Code = @Code)
        THROW 50080, 'Order not found.', 1;

    DECLARE @ShipmentCode NVARCHAR(12);
    DECLARE @ShipmentCanceledId INT = (SELECT Id FROM dbo.ShipmentStatuses WHERE Name = 'Canceled');
    DECLARE @OrderCanceledId    INT = (SELECT Id FROM dbo.OrderStatuses    WHERE Name = 'Canceled');

    WHILE 1 = 1
    BEGIN
        SET @ShipmentCode = NULL;

        SELECT TOP 1 @ShipmentCode = s.Code
        FROM dbo.Shipments s
        INNER JOIN dbo.ShipmentStatuses ss ON ss.Id = s.IdStatus
        WHERE s.CodeOrder = @Code AND ss.Name = 'Pending'
        ORDER BY s.Code;

        IF @ShipmentCode IS NULL BREAK;

        EXEC dbo.ChangeShipmentStatus @Code = @ShipmentCode, @IdStatus = @ShipmentCanceledId;
    END;

    -- Falla con 50054 si la orden tiene un envío en camino (y se deshace todo)
    EXEC dbo.ChangeOrderStatus @Code = @Code, @IdStatus = @OrderCanceledId;

    COMMIT TRANSACTION;
END;

GO

CREATE OR ALTER PROCEDURE [dbo].[CancelOrderUnsafe]
    @Code NVARCHAR(12)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    IF NOT EXISTS (SELECT 1 FROM dbo.Orders WITH (UPDLOCK, HOLDLOCK) WHERE Code = @Code)
        THROW 50080, 'Order not found.', 1;

    DECLARE @ShipmentCode NVARCHAR(12);
    DECLARE @ShipmentCanceledId INT = (SELECT Id FROM dbo.ShipmentStatuses WHERE Name = 'Canceled');
    DECLARE @OrderCanceledId    INT = (SELECT Id FROM dbo.OrderStatuses    WHERE Name = 'Canceled');

    WHILE 1 = 1
    BEGIN
        SET @ShipmentCode = NULL;

        SELECT TOP 1 @ShipmentCode = s.Code
        FROM dbo.Shipments s
        INNER JOIN dbo.ShipmentStatuses ss ON ss.Id = s.IdStatus
        WHERE s.CodeOrder = @Code AND ss.Name IN ('Pending', 'On way')
        ORDER BY s.Code;

        IF @ShipmentCode IS NULL BREAK;

        EXEC dbo.ChangeShipmentStatus @Code = @ShipmentCode, @IdStatus = @ShipmentCanceledId;
    END;

    EXEC dbo.ChangeOrderStatus @Code = @Code, @IdStatus = @OrderCanceledId;

    COMMIT TRANSACTION;
END;

GO
