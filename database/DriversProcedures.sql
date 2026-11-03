
-- ---------- Guardar conductor ----------
-- Devuelve una fila con Id y Code del conductor creado.

CREATE OR ALTER PROCEDURE [dbo].[InsertDriver]
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

    BEGIN TRANSACTION;

    DECLARE @Next INT = ISNULL((SELECT MAX(CAST(RIGHT(Code, 4) AS INT))
                                FROM dbo.Drivers WITH (UPDLOCK, HOLDLOCK)
                                WHERE Code LIKE 'DRV-[0-9][0-9][0-9][0-9]'), 0) + 1;

    IF @Next > 9999
        THROW 50062, 'No more driver codes available.', 1;

    DECLARE @Code CHAR(8) = 'DRV-' + RIGHT('0000' + CAST(@Next AS VARCHAR(4)), 4);

    INSERT INTO dbo.Drivers (Code, Name, MiddleName, LastName, SecondLastName)
    VALUES (@Code, @Name, @MiddleName, @LastName, @SecondLastName);

    DECLARE @Id INT = SCOPE_IDENTITY();

    COMMIT TRANSACTION;

    SELECT @Id AS Id, @Code AS Code;
END;

GO
