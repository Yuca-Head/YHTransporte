CREATE PROCEDURE [dbo].[InsertDepartment]
    @Name NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Departments (Name) VALUES (@Name);
END;

GO

CREATE PROCEDURE [dbo].[InsertMunicipality]
    @Name NVARCHAR(50),
    @IdDept INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Municipalities (Name, IdDept) VALUES (@Name, @IdDept);
END;

GO

CREATE PROCEDURE [dbo].[InsertAddress]
    @Details NVARCHAR(125),
    @IdMunicipality INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Addresses (Details, IdMunicipality) VALUES (@Details, @IdMunicipality);
END;

GO