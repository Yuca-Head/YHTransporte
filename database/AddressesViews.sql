ALTER VIEW dbo.AddressDetails
AS
SELECT
    a.Id,
    a.Details,

    m.Id AS MunicipalityId,
    m.Name AS MunicipalityName,
    
    d.Id AS DepartmentId,
    d.Name AS DepartmentName

FROM Addresses a
INNER JOIN Municipalities m
    ON a.IdMunicipality = m.Id
INNER JOIN Departments d
    ON m.IdDept = d.Id;
GO

GO
CREATE VIEW dbo.vw_ThirdPartyAddress
AS
SELECT
    a.Id,
    a.Details,
    a.IdMunicipality,
    m.Name AS MunicipalityName,
    d.Id AS DepartmentId,
    d.Name AS DepartmentName
FROM Addresses a
INNER JOIN Municipalities m
    ON a.IdMunicipality = m.Id
INNER JOIN Departments d
    ON m.IdDept = d.Id;

GO
CREATE VIEW dbo.vw_MunicipalityDetails
AS
SELECT
    m.Id,
    m.Name,
    d.Id AS DepartmentId,
    d.Name AS DepartmentName
FROM Municipalities m
INNER JOIN Departments d
    ON m.IdDept = d.Id;

GO