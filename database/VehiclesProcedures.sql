
-- ---------- Guardar vehículo ----------
-- Devuelve una fila con el Id del vehículo creado.

CREATE OR ALTER PROCEDURE [dbo].[InsertVehicle]
    @Plate       NVARCHAR(25),
    @Description NVARCHAR(120) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @Plate       = NULLIF(LTRIM(RTRIM(@Plate)), '');
    SET @Description = NULLIF(LTRIM(RTRIM(@Description)), '');

    IF @Plate IS NULL
        THROW 50070, 'The vehicle plate is required.', 1;

    INSERT INTO dbo.Vehicles (Plate, Description)
    VALUES (@Plate, @Description);

    SELECT SCOPE_IDENTITY() AS Id;
END;

GO
