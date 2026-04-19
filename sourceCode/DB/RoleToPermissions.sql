USE [oven]
GO

/****** Object:  Table [dbo].[RoleToPermissions]    Script Date: 19/04/2026 5:13:00 CH ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[RoleToPermissions](
	[Id] [uniqueidentifier] NOT NULL,
	[RoleId] [nvarchar](max) NULL,
	[RoleName] [nvarchar](max) NULL,
	[PermissionId] [uniqueidentifier] NOT NULL,
	[PermisionName] [nvarchar](max) NULL,
	[PermisionDescription] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedMachine] [nvarchar](max) NULL,
	[IsActived] [bit] NULL,
 CONSTRAINT [PK_RoleToPermissions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

