-- ============================================================
-- Historial de envíos por vehículo / conductor
-- ============================================================
-- Una fila por envío con su ruta (municipio de origen -> municipio de destino de la orden).
-- La usa SqlServerDeliveryRepository.GetVehicleHistory / GetDriverHistory filtrando por IdVehicle o DriverCode.

CREATE OR ALTER VIEW dbo.vw_ShipmentHistory
AS
SELECT
    s.Code              AS ShipmentCode,
    s.CodeOrder         AS OrderCode,
    s.IdStatus,
    ss.Name             AS StatusName,

    s.IdVehicle,
    v.Plate             AS VehiclePlate,

    s.IdDriver,
    d.Code              AS DriverCode,

    mo.Name             AS Origin,
    md.Name             AS Destination,

    o.CreatedAt         AS OrderCreatedAt,
    s.EstimatedPickupAt,
    s.PickedUpAt,
    s.DeliveredAt
FROM dbo.Shipments s
INNER JOIN dbo.ShipmentStatuses ss ON ss.Id = s.IdStatus
INNER JOIN dbo.Orders o            ON o.Code = s.CodeOrder
INNER JOIN dbo.Addresses ao        ON ao.Id = o.IdOriginDir
INNER JOIN dbo.Municipalities mo   ON mo.Id = ao.IdMunicipality
INNER JOIN dbo.Addresses ad        ON ad.Id = o.IdDestinationDir
INNER JOIN dbo.Municipalities md   ON md.Id = ad.IdMunicipality
LEFT JOIN dbo.Vehicles v           ON v.Id = s.IdVehicle
LEFT JOIN dbo.Drivers d            ON d.Id = s.IdDriver;

GO


-- ============================================================
-- Consultas útiles para revisar los datos de las pantallas nuevas
-- ============================================================

-- Historial de un vehículo (igual a lo que muestra el editor de vehículos)
--   SELECT * FROM dbo.vw_ShipmentHistory WHERE IdVehicle = 1 ORDER BY OrderCreatedAt DESC;

-- Historial de un conductor
--   SELECT * FROM dbo.vw_ShipmentHistory WHERE DriverCode = 'DRV-0001' ORDER BY OrderCreatedAt DESC;

-- Vehículos/conductores marcados "en uso" que NO tienen un envío activo (deberían estar libres)
--   SELECT v.* FROM dbo.Vehicles v
--   WHERE v.InUse = 1 AND NOT EXISTS (SELECT 1 FROM dbo.Shipments s
--       INNER JOIN dbo.ShipmentStatuses ss ON ss.Id = s.IdStatus
--       WHERE s.IdVehicle = v.Id AND ss.Name IN ('Pending', 'On way'));
--
--   SELECT d.* FROM dbo.Drivers d
--   WHERE d.InUse = 1 AND NOT EXISTS (SELECT 1 FROM dbo.Shipments s
--       INNER JOIN dbo.ShipmentStatuses ss ON ss.Id = s.IdStatus
--       WHERE s.IdDriver = d.Id AND ss.Name IN ('Pending', 'On way'));

-- Resumen de envíos de una orden (lo mismo que calcula el editor de órdenes)
--   SELECT ss.Name AS Status, COUNT(DISTINCT s.Code) AS Shipments,
--          COUNT(sd.Id) AS Packages, ISNULL(SUM(sd.Weight_kg), 0) AS TotalWeightKg
--   FROM dbo.Shipments s
--   INNER JOIN dbo.ShipmentStatuses ss ON ss.Id = s.IdStatus
--   LEFT JOIN dbo.ShipmentsDetails sd  ON sd.Code_Shipment = s.Code
--   WHERE s.CodeOrder = '0001-071126%'
--   GROUP BY ss.Name;

-- Paquetes cuyo producto ya no pertenece al proveedor de su orden (no debería devolver filas)
--   SELECT sd.*, o.SupplierId FROM dbo.ShipmentsDetails sd
--   INNER JOIN dbo.Shipments s ON s.Code = sd.Code_Shipment
--   INNER JOIN dbo.Orders o    ON o.Code = s.CodeOrder
--   WHERE NOT EXISTS (SELECT 1 FROM dbo.SupplierProducts sp
--                     WHERE sp.IdSupplier = o.SupplierId AND sp.IdProduct = sd.IdProduct);
