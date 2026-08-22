CREATE PROCEDURE [dbo].[GetOrdersByStatus]
    @IdStatus INT
    AS
BEGIN
    IF dbo.IsValidOrderStatusId(@IdStatus) = 0
    BEGIN
        RAISERROR('Invalid status ID.', 16, 1);
        RETURN;
    END

    SELECT *
    FROM [dbo].[Orders]
    WHERE [IdStatus] = @IdStatus;
END;

GO

CREATE PROCEDURE [dbo].[GetShipmentsByStatus]
    @IdStatus INT
    AS
BEGIN

    IF dbo.IsValidShipmentStatusId(@IdStatus) = 0
    BEGIN
        RAISERROR('Invalid status ID.', 16, 1);
        RETURN;
    END

    SELECT * FROM
    [dbo].[Shipments] WHERE [IdStatus] = @IdStatus;

END

GO

CREATE PROCEDURE [dbo].[InsertThirdParty]
    @Name NVARCHAR(120),
    @IsCustomer BIT = 0,
    @IsSupplier BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.ThirdParties VALUES (@Name, @IsSupplier, @IsCustomer);
END;

GO

CREATE PROCEDURE [dbo].[DefineOrder]

AS
BEGIN
    SET NOCOUNT ON;
END;

GO

CREATE PROCEDURE InsertShipmentsDetail
    @Code NVARCHAR(12),
    @Product INT,
    @Kg DECIMAL (10,2),
    @Amount INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NextId INT;

    SELECT @NextId = ISNULL(MAX(Id), 0) + 1
    FROM ShipmentsDetails WITH (UPDLOCK, HOLDLOCK)
    WHERE Code_Shipment = @Code;

    INSERT INTO ShipmentsDetails (Code_Shipment, Id, Weight_kg, IdProduct, ProductAmount)
    VALUES (@Code, @NextId, @Kg, @Product, @Amount);
END;