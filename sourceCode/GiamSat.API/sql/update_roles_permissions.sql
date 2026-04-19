SET NOCOUNT ON;

IF OBJECT_ID('dbo.RolePermissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RolePermissions (
        RoleId UNIQUEIDENTIFIER NOT NULL,
        PermissionId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_RolePermissions_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId)
    );
END;

IF OBJECT_ID('dbo.SecurityRoles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SecurityRoles (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SecurityRoles_Id DEFAULT NEWID(),
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(256) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_SecurityRoles_IsActive DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_SecurityRoles_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_SecurityRoles PRIMARY KEY (Id)
    );
    CREATE UNIQUE INDEX UX_SecurityRoles_Name ON dbo.SecurityRoles(Name);
END;

IF OBJECT_ID('dbo.SecurityPermissions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SecurityPermissions (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_SecurityPermissions_Id DEFAULT NEWID(),
        Code NVARCHAR(150) NOT NULL,
        Module NVARCHAR(100) NOT NULL,
        [Action] NVARCHAR(50) NOT NULL,
        Description NVARCHAR(256) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_SecurityPermissions_IsActive DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_SecurityPermissions_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT PK_SecurityPermissions PRIMARY KEY (Id)
    );
    CREATE UNIQUE INDEX UX_SecurityPermissions_Code ON dbo.SecurityPermissions(Code);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.SecurityRoles WHERE Name = 'Admin')
    INSERT INTO dbo.SecurityRoles (Name, Description) VALUES ('Admin', N'Full access');

IF NOT EXISTS (SELECT 1 FROM dbo.SecurityRoles WHERE Name = 'User')
    INSERT INTO dbo.SecurityRoles (Name, Description) VALUES ('User', N'Default operator user');

IF NOT EXISTS (SELECT 1 FROM dbo.SecurityRoles WHERE Name = 'Operator')
    INSERT INTO dbo.SecurityRoles (Name, Description) VALUES ('Operator', N'Read only user');

DECLARE @Perm TABLE(Code NVARCHAR(150), Module NVARCHAR(100), [Action] NVARCHAR(50), Description NVARCHAR(256));
INSERT INTO @Perm(Code, Module, [Action], Description)
VALUES
('Revo.View', 'Revo', 'View', N'View Revo data'),
('Revo.Create', 'Revo', 'Create', N'Create Revo data'),
('Revo.Edit', 'Revo', 'Edit', N'Edit Revo data'),
('Revo.Delete', 'Revo', 'Delete', N'Delete Revo data'),
('Revo.Export', 'Revo', 'Export', N'Export Revo data'),
('Revo.Approve', 'Revo', 'Approve', N'Approve Revo data'),
('Oven.View', 'Oven', 'View', N'View Oven data'),
('Oven.Create', 'Oven', 'Create', N'Create Oven data'),
('Oven.Edit', 'Oven', 'Edit', N'Edit Oven data'),
('Oven.Delete', 'Oven', 'Delete', N'Delete Oven data'),
('Oven.Export', 'Oven', 'Export', N'Export Oven data'),
('Oven.Approve', 'Oven', 'Approve', N'Approve Oven data');

INSERT INTO dbo.SecurityPermissions (Code, Module, [Action], Description)
SELECT p.Code, p.Module, p.[Action], p.Description
FROM @Perm p
WHERE NOT EXISTS (SELECT 1 FROM dbo.SecurityPermissions sp WHERE sp.Code = p.Code);

DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM dbo.SecurityRoles WHERE Name = 'Admin');

INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT @AdminRoleId, sp.Id
FROM dbo.SecurityPermissions sp
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.RolePermissions rp
    WHERE rp.RoleId = @AdminRoleId AND rp.PermissionId = sp.Id
);
