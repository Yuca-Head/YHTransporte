-- ============================================================
-- Vehículos y conductores: modificar + placa única
-- ============================================================
-- Códigos de error:
--   50070 Placa obligatoria (mismo número que InsertVehicle)
--   50071 Vehículo no encontrado
--   50072 No se cambia la placa de un vehículo en uso
--   50073 Placa repetida
--   50060 / 50061 Nombre / apellido obligatorios (mismos números que InsertDriver)
--   50063 Conductor no encontrado


-- ---------- Placa única ----------
-- Antes de crear el índice, revisar si ya hay placas repetidas (si devuelve filas, corregirlas primero):
--   SELECT UPPER(LTRIM(RTRIM(Plate))) AS Plate, COUNT(*) AS Times
--   FROM dbo.Vehicles GROUP BY UPPER(LTRIM(RTRIM(Plate))) HAVING COUNT(*) > 1;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Vehicles_Plate' AND object_id = OBJECT_ID('dbo.Vehicles'))
    CREATE UNIQUE INDEX [UX_Vehicles_Plate] ON [dbo].[Vehicles] ([Plate]);

GO


-- ---------- Guardar vehículo (reemplaza la versión de VehiclesProcedures.sql) ----------
-- Igual que antes, pero rechaza placas repetidas con un mensaje claro (50073).

CREATE OR ALTER PROCEDURE [dbo].[InsertVehicle]
    @Plate       NVARCHAR(25),
    @Description NVARCHAR(120) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Plate       = NULLIF(LTRIM(RTRIM(@Plate)), '');
    SET @Description = NULLIF(LTRIM(RTRIM(@Description)), '');

    IF @Plate IS NULL
        THROW 50070, 'The vehicle plate is required.', 1;

    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.Vehicles WITH (UPDLOCK, HOLDLOCK) WHERE UPPER(LTRIM(RTRIM(Plate))) = UPPER(@Plate))
        THROW 50073, 'There is already a vehicle with this plate.', 1;

    INSERT INTO dbo.Vehicles (Plate, Description)
    VALUES (@Plate, @Description);

    DECLARE @Id INT = SCOPE_IDENTITY();

    COMMIT TRANSACTION;

    SELECT @Id AS Id;
END;

GO


-- ---------- Modificar vehículo ----------

CREATE OR ALTER PROCEDURE [dbo].[UpdateVehicle]
    @Id          INT,
    @Plate       NVARCHAR(25),
    @Description NVARCHAR(120) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Plate       = NULLIF(LTRIM(RTRIM(@Plate)), '');
    SET @Description = NULLIF(LTRIM(RTRIM(@Description)), '');

    IF @Plate IS NULL
        THROW 50070, 'The vehicle plate is required.', 1;

    BEGIN TRANSACTION;

    DECLARE @CurrentPlate NVARCHAR(25), @InUse BIT;

    SELECT @CurrentPlate = Plate, @InUse = InUse
    FROM dbo.Vehicles WITH (UPDLOCK, HOLDLOCK)
    WHERE Id = @Id;

    IF @CurrentPlate IS NULL
        THROW 50071, 'Vehicle not found.', 1;

    IF @InUse = 1 AND UPPER(LTRIM(RTRIM(@CurrentPlate))) <> UPPER(@Plate)
        THROW 50072, 'The plate of a vehicle in use cannot be changed.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Vehicles WITH (UPDLOCK, HOLDLOCK)
               WHERE Id <> @Id AND UPPER(LTRIM(RTRIM(Plate))) = UPPER(@Plate))
        THROW 50073, 'There is already a vehicle with this plate.', 1;

    UPDATE dbo.Vehicles
    SET Plate       = @Plate,
        Description = @Description
    WHERE Id = @Id;

    COMMIT TRANSACTION;
END;

GO


-- ---------- Modificar conductor ----------
-- El código (DRV-0000) y InUse no se cambian aquí.

CREATE OR ALTER PROCEDURE [dbo].[UpdateDriver]
    @Code           CHAR(8),
    @Name           NVARCHAR(20),
    @MiddleName     NVARCHAR(20) = NULL,
    @LastName       NVARCHAR(15),
    @SecondLastName NVARCHAR(15) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Name           = NULLIF(LTRIM(RTRIM(@Name)), '');
    SET @MiddleName     = NULLIF(LTRIM(RTRIM(@MiddleName)), '');
    SET @LastName       = NULLIF(LTRIM(RTRIM(@LastName)), '');
    SET @SecondLastName = NULLIF(LTRIM(RTRIM(@SecondLastName)), '');

    IF @Name IS NULL
        THROW 50060, 'The driver name is required.', 1;

    IF @LastName IS NULL
        THROW 50061, 'The driver last name is required.', 1;

    UPDATE dbo.Drivers
    SET Name           = @Name,
        MiddleName     = @MiddleName,
        LastName       = @LastName,
        SecondLastName = @SecondLastName
    WHERE Code = @Code;

    IF @@ROWCOUNT = 0
        THROW 50063, 'Driver not found.', 1;
END;

GO
