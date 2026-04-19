USE [oven]
GO

/****** Object:  Table [dbo].[Permissions]    Script Date: 19/04/2026 5:12:20 CH ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Permissions](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Module] [nvarchar](max) NULL,
	[Actions] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedMachine] [nvarchar](max) NULL,
	[IsActived] [bit] NULL,
 CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

