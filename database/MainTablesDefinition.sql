CREATE TABLE [dbo].[ThirdParties] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [Name]       NVARCHAR (120) NOT NULL UNIQUE,
    [IsSupplier] BIT           NOT NULL,
    [IsCustomer] BIT           NOT NULL
    CONSTRAINT [PK_ThirdParties] PRIMARY KEY CLUSTERED ([Id] ASC)   
);


CREATE TABLE [dbo].[Addresses] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Details]       NVARCHAR (125) NOT NULL,
    [IdMunicipality ] INT  NOT NULL,
--  [Dept]          NVARCHAR (50)  NOT NULL, (DROPPED)
    [IdThirdParty] INT NOT NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Addresses_IdMunicipality] FOREIGN KEY ([IdMunicipality])
    REFERENCES [dbo].[Municipality] ([Id]),
    CONSTRAINT [FK_Addresses_ThirdParties] FOREIGN KEY ([IdThirdParty])
    REFERENCES [dbo].[ThirdParties] ([Id])
);

--Changes:
/*
*   --Now Addresses connects to other table (municipality) and municipality to Departaments
*   ALTER TABLE Addresses ALTER COLUMN Municipality INT NOT NULL;
*   EXEC sp_rename 'Addresses.Municipality', 'IdMunicipality', 'COLUMN';   
*   ALTER TABLE Addresses DROP COLUMN Dept;
*
*   --Now the new Constraint for FK
*   ALTER TABLE Addresses ADD CONSTRAINT [FK_Addresses_IdMunicipality] FOREIGN KEY ([IdMunicipality])
*   REFERENCES [dbo].[Municipality] ([Id]);
*
*    ALTER TABLE Addresses ADD [IdThirdParty] INT NOT NULL;
*    ALTER TABLE Addresses ADD CONSTRAINT [FK_Addresses_ThirdParties] FOREIGN KEY ([IdThirdParty])
*    REFERENCES [dbo].[ThirdParties] ([Id]);
*
*
*/


CREATE TABLE [dbo].[Departments] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY CLUSTERED ([Id] ASC)
);

ALTER TABLE [dbo].[Departments] ADD CONSTRAINT [Unique_Name] UNIQUE ([Name])

CREATE TABLE [dbo].[Municipality] (
    [Id]     INT           IDENTITY (1, 1) NOT NULL,
    [Name]   NVARCHAR (50) NOT NULL UNIQUE,
    [IdDept] INT           NOT NULL,
    CONSTRAINT [PK_Municipality] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Municipality_Departments] FOREIGN KEY ([IdDept]) REFERENCES [dbo].[Departments] ([Id])
);






CREATE TABLE [dbo].[Vehicles] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Plate]       NVARCHAR (25)  NOT NULL,
    [Description] NVARCHAR (120) NULL,
    CONSTRAINT [PK_Vehicles] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[Products] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Code] NVARCHAR (8) NOT NULL UNIQUE,
    [Name] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[Orders] (
    [Code]             NVARCHAR (12)  NOT NULL,
    [CreatedAt]        DATETIMEOFFSET (7) NOT NULL,
    [SupplierId]       INT            NOT NULL,
    [CustomerId]       INT            NOT NULL, 
    [IdOriginDir]      INT            NOT NULL,
    [IdDestinationDir] INT            NOT NULL,
    [IdStatus]         INT            CONSTRAINT [DEFAULT_Order_IdStatus] DEFAULT 1 NOT NULL,
    [Description]      NVARCHAR (150) NULL, 
    CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED ([Code] ASC),
    CONSTRAINT [FK_Order_Addresses_Destination] FOREIGN KEY ([IdDestinationDir]) REFERENCES [dbo].[Addresses] ([Id]),
    CONSTRAINT [FK_Order_Addresses_Origin] FOREIGN KEY ([IdOriginDir]) REFERENCES [dbo].[Addresses] ([Id]),
    CONSTRAINT [FK_Order_ThirdParties_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[ThirdParties] ([Id]),
    CONSTRAINT [FK_Order_ThirdParties_Supplier] FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[ThirdParties] ([Id]),
    CONSTRAINT [FK_Order_OrderStatuses] FOREIGN KEY ([IdStatus]) REFERENCES [dbo].[OrderStatuses] ([Id]),
    CONSTRAINT [CreatedTime_Order] DEFAULT SYSDATETIMEOFFSET() FOR [CreatedAt]
);


CREATE TABLE [dbo].[SupplierProducts] (
    [IdProduct]  INT NOT NULL,
    [IdSupplier] INT NOT NULL,
    CONSTRAINT [PK_SupplierProducts] PRIMARY KEY CLUSTERED ([IdProduct] ASC, [IdSupplier] ASC),
    CONSTRAINT [FK_SupplierProducts_Products] FOREIGN KEY ([IdProduct]) REFERENCES [dbo].[Products] ([Id]) ,
    CONSTRAINT [FK_SupplierProducts_ThirdParties] FOREIGN KEY ([IdSupplier]) REFERENCES [dbo].[ThirdParties] ([Id])
);

CREATE TABLE [dbo].[Drivers] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [Code]           CHAR (8)      NOT NULL UNIQUE,
    [Name]           NVARCHAR (20) NOT NULL,
    [MiddleName]     NVARCHAR (20) NULL,
    [LastName]       NVARCHAR (15) NOT NULL,
    [SecondLastName] NVARCHAR (15) NULL,
    CONSTRAINT [PK_Drivers] PRIMARY KEY CLUSTERED ([Id] ASC)
);


CREATE TABLE [dbo].[OrderStatuses] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (25) NOT NULL UNIQUE,
    CONSTRAINT [PK_OrderStatuses] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[ShipmentStatuses] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (25) NOT NULL UNIQUE,
    CONSTRAINT [PK_ShipmentStatuses] PRIMARY KEY CLUSTERED ([Id] ASC)
);



CREATE TABLE [dbo].[Shipments] (
    [Code]                NVARCHAR (12)      NOT NULL,
    [CodeOrder]           NVARCHAR (12)      NOT NULL,
    [IdVehicle]           INT                NULL,
    [IdDriver]            INT                NULL,
    [EstimatedPickupAt] DATETIMEOFFSET (7)  NULL,
    [PickedUpAt]        DATETIMEOFFSET (7) NULL,
    [DeliveredAt]      DATETIMEOFFSET (7) NULL,
    [IdStatus]            INT                NOT NULL    CONSTRAINT [DEFAULT_Shipment_IdStatus] DEFAULT 1,
    [Description]         NVARCHAR (120)     NULL,
    CONSTRAINT [PK_Shipments] PRIMARY KEY CLUSTERED ([Code] ASC),
    CONSTRAINT [FK_Shipments_Drivers] FOREIGN KEY ([IdDriver]) REFERENCES [dbo].[Drivers] ([Id]),
    CONSTRAINT [FK_Shipments_Orders] FOREIGN KEY ([CodeOrder]) REFERENCES [dbo].[Orders] ([Code]),
    CONSTRAINT [FK_Shipments_ShipmentStatuses] FOREIGN KEY ([IdStatus]) REFERENCES [dbo].[ShipmentStatuses] ([Id]),
    CONSTRAINT [FK_Shipments_Vehicles] FOREIGN KEY ([IdVehicle]) REFERENCES [dbo].[Vehicles] ([Id])
);


CREATE TABLE [dbo].[ShipmentsDetails] (
    [Code_Shipment] NVARCHAR (12) NOT NULL,
    [Id]            INT           NOT NULL,
    [IdProduct]     INT           NOT NULL,
    [Weight_kg]     DECIMAL (10,2)  NOT NULL,
    [ProductAmount] INT           NOT NULL,
    CONSTRAINT [PK_ShipmentsDetails] PRIMARY KEY CLUSTERED ([Code_Shipment] ASC, [Id] ASC),
    CONSTRAINT [FK_ShipmentsDetails_Products] FOREIGN KEY ([IdProduct]) REFERENCES [dbo].[Products] ([Id]),
    CONSTRAINT [FK_ShipmentsDetails_Shipments] FOREIGN KEY ([Code_Shipment]) REFERENCES [dbo].[Shipments] ([Code])
);






INSERT INTO [dbo].[ShipmentStatuses] VALUES ('Pending');--Other statuses will be added in future vesions


INSERT INTO [dbo].[OrderStatuses] VALUES ('Pending'), ('In Progress'), ('Completed');

DELETE FROM ThirdParties WHERE Id = 1;

DBCC CHECKIDENT ('ThirdParties', RESEED, 0);   
SELECT * FROM ThirdParties;    
