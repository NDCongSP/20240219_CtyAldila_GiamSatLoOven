USE [oven]
GO

/****** Object:  Table [dbo].[FT09]    Script Date: 2/10/2026 2:50:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FT09](
	[Id] [uniqueidentifier] NOT NULL,
	[CreatedAt] [datetime2](7) NULL,
	[CreatedMachine] [nvarchar](max) NULL,
	[RevoId] [int] NULL,
	[RevoName] [nvarchar](max) NULL,
	[Work] [nvarchar](max) NULL,
	[Part] [nvarchar](max) NULL,
	[Rev] [nvarchar](max) NULL,
	[ColorCode] [nvarchar](max) NULL,
	[Mandrel] [nvarchar](max) NULL,
	[MandrelStart] [nvarchar](max) NULL,
	[StepId] [int] NULL,
	[StepName] [nvarchar](max) NULL,
	[StartedAt] [datetime2](7) NULL,
	[EndedAt] [datetime2](7) NULL,
	[ShaftNum] [uniqueidentifier] NULL,
 CONSTRAINT [PK_FT09] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

