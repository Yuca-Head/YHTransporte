CREATE FUNCTION [dbo].[IsValidOrderStatusId] (@IdStatus INT)
RETURNS BIT
AS  
BEGIN
    DECLARE @IsValid BIT;

    IF @IdStatus IS NULL OR @IdStatus < 0 OR
        NOT EXISTS (SELECT 1 FROM [dbo].[OrderStatuses] WHERE [Id] = @IdStatus)
       
    BEGIN
        SET @IsValid = 0; -- Invalid status ID
    END
    ELSE
    BEGIN
        SET @IsValid = 1; -- Valid status ID
    END

    RETURN @IsValid;
END;

GO

CREATE FUNCTION [dbo].[IsValidShipmentStatusId] (@IdStatus INT)
RETURNS BIT
AS  
BEGIN
    DECLARE @IsValid BIT;

    IF @IdStatus IS NULL OR @IdStatus < 0 OR
        NOT EXISTS (SELECT 1 FROM [dbo].[ShipmentStatuses] WHERE [Id] = @IdStatus)
    BEGIN
        SET @IsValid = 0; -- Invalid status ID
    END
    ELSE
    BEGIN
        SET @IsValid = 1; -- Valid status ID
    END

    RETURN @IsValid;
END;

GO