CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
-----------------------------------------------------------
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
--------------------------------------------------------------
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
------------------------------------------------------------------
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
---------------------------------------------------------------------

----------------------------------------------------------
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
--------------------------------------------------------------------------------------------
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
----------------------------------------------------------------
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO

ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
-------------------------------------------------------------------------------------------
CREATE TABLE [dbo].[FT01](
	[Id] [uniqueidentifier] NOT NULL,
	[C000] [nvarchar](max) NULL,
	[C001] [nvarchar](max) NULL,
	[Actived] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_FT01] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[FT01] ADD  CONSTRAINT [DF_FT01_Id]  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[FT01] ADD  CONSTRAINT [DF_FT01_Actived]  DEFAULT ((1)) FOR [Actived]
GO

ALTER TABLE [dbo].[FT01] ADD  CONSTRAINT [DF_FT01_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
------------------------------------------------------------------------------------------
CREATE TABLE [dbo].[FT02](
	[Id] [uniqueidentifier] NOT NULL,
	[C000] [nvarchar](max) NULL,
	[Actived] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedMachine] [nvarchar](max) NULL,
 CONSTRAINT [PK_FT02] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[FT02] ADD  CONSTRAINT [DF_FT02_Id]  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[FT02] ADD  CONSTRAINT [DF__FT02__Actived__04E4BC85]  DEFAULT ((1)) FOR [Actived]
GO

ALTER TABLE [dbo].[FT02] ADD  CONSTRAINT [DF_FT02_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[FT02] ADD  CONSTRAINT [DF_FT02_CreatedMachine]  DEFAULT (host_name()) FOR [CreatedMachine]
GO
-------------------------------------------------------------------------
CREATE TABLE [dbo].[FT03](
	[Id] [uniqueidentifier] NOT NULL,
	[OvenId] [int] NOT NULL,
	[OvenName] [nvarchar](max) NULL,
	[Temperature] [float] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Actived] [int] NOT NULL,
	[CreatedMachine] [nvarchar](max) NULL,
 CONSTRAINT [PK_FT03] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[FT03] ADD  CONSTRAINT [DF_FT03_Id]  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[FT03] ADD  CONSTRAINT [DF_FT03_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[FT03] ADD  CONSTRAINT [DF_FT03_IsActive]  DEFAULT ((1)) FOR [Actived]
GO

ALTER TABLE [dbo].[FT03] ADD  CONSTRAINT [DF_FT03_CreatedMachine]  DEFAULT (host_name()) FOR [CreatedMachine]
GO
---------------------------------------------------------
CREATE TABLE [dbo].[FT04](
	[Id] [uniqueidentifier] NOT NULL,
	[OvenId] [int] NOT NULL,
	[OvenName] [nvarchar](max) NULL,
	[Temperature] [float] NOT NULL,
	[StartTime] [datetime2](7) NOT NULL,
	[EndTime] [datetime2](7) NULL,
	[Actived] [int] NOT NULL,
	[CreatedMachine] [nvarchar](max) NULL,
	[ZIndex] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_FT04] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[FT04] ADD  CONSTRAINT [DF_FT04_Id]  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[FT04] ADD  CONSTRAINT [DF_FT04_Actived]  DEFAULT ((1)) FOR [Actived]
GO

ALTER TABLE [dbo].[FT04] ADD  CONSTRAINT [DF_FT04_CreatedMachine]  DEFAULT (host_name()) FOR [CreatedMachine]
GO

ALTER TABLE [dbo].[FT04] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [ZIndex]
GO
----------------------------------------------------
CREATE TABLE [dbo].[FT05](
	[Id] [uniqueidentifier] NOT NULL,
	[OvenId] [int] NOT NULL,
	[OvenName] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[ACK] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[ACKDate] [datetime2](7) NULL,
	[Actived] [int] NOT NULL,
 CONSTRAINT [PK_FT05] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[FT05] ADD  CONSTRAINT [DF_FT05_Id]  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[FT05] ADD  CONSTRAINT [DF_FT05_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[FT05] ADD  CONSTRAINT [DF_FT05_Actived]  DEFAULT ((1)) FOR [Actived]
GO


-- =============================================
CREATE PROCEDURE [dbo].[sp_FT01GetAll]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT *
	FROM FT01
	WHERE Actived = 1
END
go

CREATE PROCEDURE [dbo].[sp_FT02GetAll]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT *
	FROM FT02
	WHERE Actived = 1
END
GO

CREATE PROCEDURE [dbo].[sp_FT02Insert]
	-- Add the parameters for the stored procedure here
	@c000 nvarchar(max)
	,@createdDate datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	--SET NOCOUNT ON;

    INSERT INTO [dbo].[FT02]
           ([C000]
		   ,[CreatedDate]
           )
     VALUES
           (@c000
		   ,@createdDate)
END
GO

CREATE PROCEDURE [dbo].[sp_FT03GetAll]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT *
	FROM FT03
	WHERE Actived = 1
END
GO

CREATE PROCEDURE [dbo].[sp_FT03Insert]
	-- Add the parameters for the stored procedure here
	@ovenId int
	,@ovenName varchar(500)
	,@temperature float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	--SET NOCOUNT ON;

    INSERT INTO [dbo].[FT03]
           (
           [OvenId]
           ,[OvenName]
           ,[Temperature])
     VALUES
           (@ovenId
		   ,@ovenName
		   ,@temperature)
END
GO

CREATE PROCEDURE [dbo].[sp_FT04GetAll]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT *
	FROM FT04
	WHERE Actived = 1
END
GO

CREATE PROCEDURE [dbo].[sp_FT04Insert]
	-- Add the parameters for the stored procedure here
	@ovenId int
	,@ovenName varchar(500)
	,@temperature float
	,@startTime datetime
	,@endTime datetime = null
	,@zIndex uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	--SET NOCOUNT ON;

    INSERT INTO [dbo].[FT04]
           (
           [OvenId]
           ,[OvenName]
           ,[Temperature]
           ,[StartTime]
           ,[EndTime]
           ,[ZIndex])
     VALUES
           (@ovenId
		   ,@ovenName
		   ,@temperature
		   ,@startTime
		   ,@endTime
		   ,@zIndex)
END
GO

CREATE PROCEDURE [dbo].[sp_FT05GetAll]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT *
	FROM FT05
	WHERE Actived = 1
END
GO

CREATE PROCEDURE [dbo].[sp_FT05Insert]
	-- Add the parameters for the stored procedure here
	@ovenId int
	,@ovenName varchar(500)
	,@description varchar(max)
	,@ack int = 0
	,@ackDate datetime = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	--SET NOCOUNT ON;

   INSERT INTO [dbo].[FT05]
           (
           [OvenId]
           ,[OvenName]
           ,[Description]
           ,[ACK]
           ,[ACKDate])
     VALUES
           (@ovenId
		   ,@ovenName
		   ,@description
		   ,@ack
		   ,@ackDate
		   )
END
GO