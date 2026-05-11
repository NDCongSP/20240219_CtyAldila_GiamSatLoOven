USE [master]
GO
/****** Object:  Database [ALD_MFG1]    Script Date: 5/11/2026 3:53:27 PM ******/
CREATE DATABASE [ALD_MFG1]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ALD_MFG_Data', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\ALD_MFG_Data.mdf' , SIZE = 22813056KB , MAXSIZE = UNLIMITED, FILEGROWTH = 10%)
 LOG ON 
( NAME = N'ALD_MFG_Log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\ALD_MFG_Log.ldf' , SIZE = 331776KB , MAXSIZE = 2048GB , FILEGROWTH = 1024KB )
GO
ALTER DATABASE [ALD_MFG1] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ALD_MFG1].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ALD_MFG1] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ALD_MFG1] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ALD_MFG1] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ALD_MFG1] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ALD_MFG1] SET ARITHABORT OFF 
GO
ALTER DATABASE [ALD_MFG1] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [ALD_MFG1] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ALD_MFG1] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ALD_MFG1] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ALD_MFG1] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ALD_MFG1] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ALD_MFG1] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ALD_MFG1] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ALD_MFG1] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ALD_MFG1] SET  DISABLE_BROKER 
GO
ALTER DATABASE [ALD_MFG1] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ALD_MFG1] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ALD_MFG1] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ALD_MFG1] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ALD_MFG1] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ALD_MFG1] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [ALD_MFG1] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ALD_MFG1] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [ALD_MFG1] SET  MULTI_USER 
GO
ALTER DATABASE [ALD_MFG1] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ALD_MFG1] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ALD_MFG1] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ALD_MFG1] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [ALD_MFG1] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [ALD_MFG1] SET QUERY_STORE = OFF
GO
USE [ALD_MFG1]
GO
/****** Object:  User [superuser]    Script Date: 5/11/2026 3:53:27 PM ******/
CREATE USER [superuser] FOR LOGIN [superuser] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [mfg]    Script Date: 5/11/2026 3:53:27 PM ******/
CREATE USER [mfg] FOR LOGIN [mfg] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [ALD_VM4\serviceuser]    Script Date: 5/11/2026 3:53:27 PM ******/
CREATE USER [ALD_VM4\serviceuser] FOR LOGIN [ALD_VM4\serviceuser] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [mfg]
GO
ALTER ROLE [db_owner] ADD MEMBER [ALD_VM4\serviceuser]
GO
ALTER ROLE [db_backupoperator] ADD MEMBER [ALD_VM4\serviceuser]
GO
/****** Object:  Table [dbo].[ButtStop]    Script Date: 5/11/2026 3:53:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ButtStop](
	[L1] [nvarchar](50) NULL,
	[L2] [nvarchar](50) NULL,
	[L3] [nvarchar](50) NULL,
	[L4] [nvarchar](50) NULL,
	[L5] [nvarchar](50) NULL,
	[L6] [nvarchar](50) NULL,
	[L7] [nvarchar](50) NULL,
	[L8] [nvarchar](50) NULL,
	[L9] [nvarchar](50) NULL,
	[L10] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ClampingPressure]    Script Date: 5/11/2026 3:53:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ClampingPressure](
	[L1] [nvarchar](50) NULL,
	[L2] [nvarchar](50) NULL,
	[L3] [nvarchar](50) NULL,
	[L4] [nvarchar](50) NULL,
	[L5] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ConfigurationSetting]    Script Date: 5/11/2026 3:53:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConfigurationSetting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FrequencyStandard_UpperLimit] [real] NULL,
	[FrequencyStandard_LowerLimit] [real] NULL,
	[PartZMike_DiamUL] [real] NULL,
	[PartZMike_DiamLL] [real] NULL,
	[PartZMike_MinUnder] [real] NULL,
	[PartZMike_MaxOver] [real] NULL,
	[SwingWeight_CenterOfGravityUperLimit123] [real] NULL,
	[SwingWeight_CenterOfGravityLowerLimit123] [real] NULL,
	[SwingWeight_CenterOfGravityUperLimit4] [real] NULL,
	[SwingWeight_CenterOfGravityLowerLimit4] [real] NULL,
	[SwingWeight_CenterOfGravityUperLimit5] [real] NULL,
	[SwingWeight_CenterOfGravityLowerLimit5] [real] NULL,
	[SwingWeight_WeightUperLimit] [real] NULL,
	[SwingWeight_WeightLowerLimit] [real] NULL,
	[LengthStraightness_UpperLimit] [real] NULL,
	[LengthStraightness_LowerLimit] [real] NULL,
	[LengthStraightness_Standard] [real] NULL,
	[LengthStraightness_Straightness] [real] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DatalogFrequency]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DatalogFrequency](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Station] [nvarchar](50) NULL,
	[ShaftNum] [int] NULL,
	[DateTime] [datetime] NULL,
	[Part] [nvarchar](50) NULL,
	[WorkOrder] [nvarchar](50) NULL,
	[Standard] [nvarchar](50) NULL,
	[BSL] [nvarchar](50) NULL,
	[Weight] [nvarchar](50) NULL,
	[Reading] [nvarchar](50) NULL,
	[UL] [nvarchar](50) NULL,
	[LL] [nvarchar](50) NULL,
	[PassFail] [nvarchar](50) NULL,
	[BuildType] [nvarchar](50) NULL,
	[ShaftType] [nvarchar](50) NULL,
	[IsCalib] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataLogLenght]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataLogLenght](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Station] [nvarchar](50) NULL,
	[ShaftNum] [int] NULL,
	[DateTime] [datetime] NULL,
	[Part] [nvarchar](50) NULL,
	[WorkOrder] [nvarchar](50) NULL,
	[LenghtReading] [nvarchar](50) NULL,
	[LenghtMin] [nvarchar](50) NULL,
	[LenghtMax] [nvarchar](50) NULL,
	[PassFail] [nvarchar](50) NULL,
	[BuildType] [nvarchar](50) NULL,
	[ShaftType] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataLogStraightness]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataLogStraightness](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Station] [nvarchar](50) NULL,
	[ShaftNum] [int] NULL,
	[DateTime] [datetime] NULL,
	[Part] [nvarchar](50) NULL,
	[WorkOrder] [nvarchar](50) NULL,
	[StraightnessReading] [nvarchar](50) NULL,
	[Straightness] [nvarchar](50) NULL,
	[PassFail] [nvarchar](50) NULL,
	[BuildType] [nvarchar](50) NULL,
	[ShaftType] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataLogSwingWeight]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataLogSwingWeight](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Station] [nvarchar](50) NULL,
	[ShaftNum] [int] NULL,
	[DateTime] [datetime] NULL,
	[Part] [nvarchar](50) NULL,
	[WorkOrder] [nvarchar](50) NULL,
	[CenterOfGravityReading] [nvarchar](50) NULL,
	[SwingWeightReading] [nvarchar](50) NULL,
	[MeansurementType] [nvarchar](50) NULL,
	[CenterOfGravityUL] [nvarchar](50) NULL,
	[CenterOfGravityLL] [nvarchar](50) NULL,
	[SwingWeightUL] [nvarchar](50) NULL,
	[SwingWeightLL] [nvarchar](50) NULL,
	[PassFail] [nvarchar](50) NULL,
	[BuildType] [nvarchar](50) NULL,
	[ShaftType] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DatalogZMike]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DatalogZMike](
	[Station] [nvarchar](50) NULL,
	[ShaftNum] [int] NULL,
	[DateTime] [datetime] NULL,
	[Part] [nvarchar](50) NULL,
	[MeasType] [nvarchar](50) NULL,
	[WorkOrder] [nvarchar](50) NULL,
	[DiamReading] [nvarchar](50) NULL,
	[RangeReading] [nvarchar](50) NULL,
	[MaxOver] [nvarchar](50) NULL,
	[MinUnder] [nvarchar](50) NULL,
	[DiamUL] [nvarchar](50) NULL,
	[DiamLL] [nvarchar](50) NULL,
	[MaxOverUL] [nvarchar](50) NULL,
	[MinUnderLL] [nvarchar](50) NULL,
	[PassFail] [nvarchar](50) NULL,
	[BuildType] [nvarchar](50) NULL,
	[ShaftType] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Flex]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Flex](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Task] [nvarchar](50) NULL,
 CONSTRAINT [PK_Flex] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Flex] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Flex_1] UNIQUE NONCLUSTERED 
(
	[Task] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Flex_Standard]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Flex_Standard](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Number] [nvarchar](25) NOT NULL,
	[Flex_Val] [real] NULL,
	[Hole_Number] [smallint] NULL,
	[Clamp_To_Wt] [nvarchar](10) NULL,
	[Clamp_To_Sensor] [nvarchar](10) NULL,
	[Clamp_To_Tip] [nvarchar](10) NULL,
 CONSTRAINT [PK_Flex_Standard] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Flex_Standard] UNIQUE NONCLUSTERED 
(
	[Number] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Freq_Standard]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Freq_Standard](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Number] [nvarchar](18) NULL,
	[UL1] [real] NULL,
	[LL1] [real] NULL,
	[UL2] [real] NULL,
	[LL2] [real] NULL,
	[UL3] [real] NULL,
	[LL3] [real] NULL,
	[UL4] [real] NULL,
	[LL4] [real] NULL,
	[UL5] [real] NULL,
	[LL5] [real] NULL,
	[UL6] [real] NULL,
	[LL6] [real] NULL,
	[UL7] [real] NULL,
	[LL7] [real] NULL,
	[UL8] [real] NULL,
	[LL8] [real] NULL,
	[UL9] [real] NULL,
	[LL9] [real] NULL,
	[UL10] [real] NULL,
	[LL10] [real] NULL,
	[WT] [real] NULL,
	[CP] [real] NULL,
	[PD] [real] NULL,
	[OD] [real] NULL,
	[Lt] [real] NULL,
 CONSTRAINT [PK_Freq_Standard] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Freq_Standard] UNIQUE NONCLUSTERED 
(
	[Number] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FreqStandardDelta]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FreqStandardDelta](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ZMikeId] [int] NOT NULL,
	[Delta] [real] NULL,
	[FreqId] [int] NOT NULL,
	[FreqBSL] [real] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Frequency]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Frequency](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Task] [nvarchar](50) NULL,
 CONSTRAINT [PK_Frequency] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Frequency] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Frequency_1] UNIQUE NONCLUSTERED 
(
	[Task] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Part]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Part](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Number] [nvarchar](18) NOT NULL,
	[Flex_LL] [real] NULL,
	[Flex_UL] [real] NULL,
	[SW_LL] [real] NULL,
	[SW_UL] [real] NULL,
	[SW_Wt_LL] [real] NULL,
	[SW_Wt_UL] [real] NULL,
	[SW_Meas_Type] [nvarchar](25) NULL,
	[Freq_LL] [real] NULL,
	[Freq_UL] [real] NULL,
	[Freq_Std] [int] NULL,
	[Freq_BSL] [real] NULL,
	[Freq_Wt] [smallint] NULL,
 CONSTRAINT [PK_Part] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Part] UNIQUE NONCLUSTERED 
(
	[Number] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PartDeltas]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartDeltas](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PartId] [int] NOT NULL,
	[ZMikeId] [int] NOT NULL,
	[Delta] [real] NULL,
	[User] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PartNewSetting]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartNewSetting](
	[PartId] [int] NOT NULL,
	[LLl] [real] NULL,
	[LUl] [real] NULL,
	[LStd] [real] NULL,
	[FR] [real] NULL,
	[TL] [bit] NULL,
	[Freq_CP] [real] NULL,
	[Freq_PD] [real] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PartZM]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartZM](
	[PartID] [int] NULL,
	[ZMID] [int] NULL,
	[Diam_LL] [real] NULL,
	[Diam_UL] [real] NULL,
	[Min_Under] [real] NULL,
	[Max_Over] [real] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PartZM_ID]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartZM_ID](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PartID] [int] NOT NULL,
	[ZMID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PartZM_NewUpdate]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartZM_NewUpdate](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PartID] [int] NULL,
	[ZMID] [int] NULL,
	[Diam_LL] [real] NULL,
	[Diam_UL] [real] NULL,
	[Min_Under] [real] NULL,
	[Max_Over] [real] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PullDown]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PullDown](
	[L1] [nvarchar](50) NULL,
	[L2] [nvarchar](50) NULL,
	[L3] [nvarchar](50) NULL,
	[L4] [nvarchar](50) NULL,
	[L5] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SWeight]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SWeight](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Task] [nvarchar](50) NULL,
 CONSTRAINT [PK_SWeight] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_SWeight] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_SWeight_1] UNIQUE NONCLUSTERED 
(
	[Task] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDataLog]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDataLog](
	[Id] [uniqueidentifier] NOT NULL,
	[Station] [nvarchar](100) NULL,
	[ShaftNumber] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[WorkOrder] [nvarchar](100) NULL,
	[Part] [nvarchar](100) NULL,
	[Freq01Reading] [float] NULL,
	[MotorSandingSpeed] [int] NULL,
	[Freq02Reading] [float] NULL,
	[MotorPolishingSpeed] [int] NULL,
	[FreqTarget] [float] NULL,
	[FormulaGId] [int] NULL,
	[FormulaPoId] [int] NULL,
	[LogStyle] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDataLogPolishing]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDataLogPolishing](
	[Id] [uniqueidentifier] NULL,
	[Station] [nvarchar](100) NULL,
	[ShaftNumber] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[Part] [nvarchar](50) NULL,
	[WorkOrder] [nvarchar](50) NULL,
	[FreqReading] [float] NULL,
	[FreqTarget] [float] NULL,
	[MortorPolishing] [float] NULL,
	[FormulaPO] [int] NULL,
	[LogType] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDataLogSanding]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDataLogSanding](
	[Id] [uniqueidentifier] NOT NULL,
	[Station] [nvarchar](100) NULL,
	[ShaftNumber] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[WorkOrder] [nvarchar](100) NULL,
	[Part] [nvarchar](100) NULL,
	[Freq01Reading] [float] NULL,
	[MotorSandingSpeed] [float] NULL,
	[Freq02Reading] [float] NULL,
	[FreqTarget] [float] NULL,
	[FormulaGId] [int] NULL,
	[LogStyle] [nvarchar](50) NULL,
 CONSTRAINT [PK_tblDataLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblDataLogTipOD]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblDataLogTipOD](
	[Id] [uniqueidentifier] NOT NULL,
	[Station] [nvarchar](100) NULL,
	[ShaftNumber] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[Part] [nvarchar](50) NULL,
	[WorkOrder] [nvarchar](50) NULL,
	[DiamReading] [float] NULL,
	[MeasType] [nvarchar](50) NULL,
	[DiamLL] [float] NULL,
	[DiamUL] [float] NULL,
	[PassFail] [nvarchar](20) NULL,
	[LogType] [nvarchar](50) NULL,
 CONSTRAINT [PK_tblDataLogOD] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblFormulaG]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblFormulaG](
	[Id] [int] NOT NULL,
	[U] [int] NULL,
	[V] [int] NULL,
	[X] [float] NULL,
	[Y] [float] NULL,
	[Z] [int] NULL,
	[P] [float] NULL,
 CONSTRAINT [PK_tblFormulaG] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblFormulaPo]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblFormulaPo](
	[Id] [int] NOT NULL,
	[U] [int] NULL,
	[V] [int] NULL,
	[X] [float] NULL,
	[Y] [float] NULL,
	[Z] [int] NULL,
	[P] [float] NULL,
 CONSTRAINT [PK_tblFormulaPo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblTipOdFreq]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblTipOdFreq](
	[Id] [uniqueidentifier] NOT NULL,
	[ItemNumber] [nvarchar](100) NULL,
	[FreqTarget] [float] NULL,
	[DiamLL] [float] NULL,
	[DiamUl] [float] NULL,
	[TipOdLength] [nvarchar](500) NULL,
	[FormulaGId] [int] NULL,
	[FormulaPoId] [int] NULL,
 CONSTRAINT [PK_tblTipOdFreq] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[useraccount]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[useraccount](
	[User] [nvarchar](50) NOT NULL,
	[Pass] [nvarchar](50) NULL,
	[Role] [nvarchar](50) NULL,
 CONSTRAINT [PK_useraccount] PRIMARY KEY CLUSTERED 
(
	[User] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Weight]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Weight](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Value] [nvarchar](50) NULL,
 CONSTRAINT [PK_Weight] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_Weight] UNIQUE NONCLUSTERED 
(
	[Value] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ZMike]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ZMike](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Task] [nvarchar](50) NULL,
 CONSTRAINT [PK_ZMike] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_ZMike] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [IX_ZMike_1] UNIQUE NONCLUSTERED 
(
	[Task] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ZMikeDelta]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ZMikeDelta](
	[ZMikeId] [int] NOT NULL,
	[Delta] [real] NULL,
	[Skip] [int] NOT NULL,
	[Take] [int] NOT NULL,
	[Count] [int] NOT NULL,
	[TimeActive] [time](7) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ZMikeRealTime]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ZMikeRealTime](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Station] [nvarchar](50) NULL,
	[ShaftNum] [int] NULL,
	[DateTime] [datetime] NULL,
	[Part] [nvarchar](50) NULL,
	[MeasType] [nvarchar](50) NULL,
	[WorkOrder] [nvarchar](50) NULL,
	[DiamReading] [nvarchar](50) NULL,
	[RangeReading] [nvarchar](50) NULL,
	[MaxOver] [nvarchar](50) NULL,
	[MinUnder] [nvarchar](50) NULL,
	[DiamUL] [nvarchar](50) NULL,
	[DiamLL] [nvarchar](50) NULL,
	[MaxOverUL] [nvarchar](50) NULL,
	[MinUnderLL] [nvarchar](50) NULL,
	[PassFail] [nvarchar](50) NULL,
	[BuildType] [nvarchar](50) NULL,
	[ShaftType] [nvarchar](50) NULL,
	[Sensor] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ZMmeasType]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ZMmeasType](
	[ID] [int] NOT NULL,
	[Name] [nvarchar](25) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Index [IX_DatalogZMike_DateTime]    Script Date: 5/11/2026 3:53:28 PM ******/
CREATE NONCLUSTERED INDEX [IX_DatalogZMike_DateTime] ON [dbo].[DatalogZMike]
(
	[DateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblDataLogPolishing] ADD  CONSTRAINT [DF_tblDataLogPolishing_Id]  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[tblDataLogPolishing] ADD  CONSTRAINT [DF_tblDataLogPolishing_Station]  DEFAULT (N'Auto Polishing') FOR [Station]
GO
ALTER TABLE [dbo].[tblDataLogPolishing] ADD  CONSTRAINT [DF_tblDataLogPolishing_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[tblDataLogSanding] ADD  CONSTRAINT [DF_tblDataLog_Id]  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[tblDataLogSanding] ADD  CONSTRAINT [DF_tblDataLogSanding_Station]  DEFAULT (N'Auto Sanding') FOR [Station]
GO
ALTER TABLE [dbo].[tblDataLogSanding] ADD  CONSTRAINT [DF_tblDataLog_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[tblDataLogTipOD] ADD  CONSTRAINT [DF_tblDataLogOD_Id]  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[tblDataLogTipOD] ADD  CONSTRAINT [DF_tblDataLogTipOD_Station]  DEFAULT (N'Auto Tip OD') FOR [Station]
GO
ALTER TABLE [dbo].[tblDataLogTipOD] ADD  CONSTRAINT [DF_tblDataLogOD_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[tblTipOdFreq] ADD  CONSTRAINT [DF_tblTipOdFreq_Id]  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[PartNewSetting]  WITH CHECK ADD  CONSTRAINT [FK_PartNewSetting_Part] FOREIGN KEY([PartId])
REFERENCES [dbo].[Part] ([ID])
GO
ALTER TABLE [dbo].[PartNewSetting] CHECK CONSTRAINT [FK_PartNewSetting_Part]
GO
/****** Object:  StoredProcedure [dbo].[SearchFrequency]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SearchFrequency]
    @DateFrom DATE,
    @DateTo DATE,
    @Part NVARCHAR(50),
    @StationName NVARCHAR(50),
    @ShaftType NVARCHAR(50),
	@Take int,
	@skip int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Station]
        ,[ShaftNum]
        ,[DateTime]
        ,[Part] 
        ,[WorkOrder]
		,[Standard]
        , CASE 
        WHEN CAST(BSL AS FLOAT) = 177.8 THEN 180 -- Xử lý riêng trường hợp 177.8
        ELSE ROUND(CAST(BSL AS FLOAT), 0) -- Làm tròn theo tiêu chuẩn
    END AS BSL
        ,[Weight]
        ,[Reading]
        ,[UL]
        ,[LL]
        ,[PassFail]
        ,[BuildType]
        ,[ShaftType]
    FROM [DatalogFrequency]
    WHERE [DateTime] >= @DateFrom
        AND [DateTime] < @DateTo
		AND (@Part is null or [Part] like '%' + @Part + '%')
		AND (@StationName is null or [Station] like '%' + @StationName + '%')
		AND [ShaftType] = @ShaftType
	order by [WorkOrder], [Part], [ShaftNum]
	OFFSET     @skip ROWS  
	FETCH NEXT @Take ROWS ONLY; 
END;
GO
/****** Object:  StoredProcedure [dbo].[SearchFrequencyCount]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SearchFrequencyCount]
    @DateFrom DATE,
    @DateTo DATE,
    @Part NVARCHAR(50),
    @StationName NVARCHAR(50),
    @ShaftType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT count (1) 
    FROM [DatalogFrequency]
    WHERE [DateTime] >= @DateFrom
        AND [DateTime] < @DateTo
		AND (@Part is null or [Part] like '%' + @Part + '%')
		AND (@StationName is null or [Station] like '%' + @StationName + '%')
		AND [ShaftType] = @ShaftType
END;
GO
/****** Object:  StoredProcedure [dbo].[SearchZMikeReport]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SearchZMikeReport]
    @DateFrom DATE,
    @DateTo DATE,
    @Part NVARCHAR(50),
    @StationName NVARCHAR(50),
    @ShaftType NVARCHAR(50),
	@Take int,
	@skip int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Station]
        ,[ShaftNum]
        ,[DateTime]
        ,[Part] 
        ,[MeasType]
        ,[WorkOrder]
        ,CONVERT(NVARCHAR(50), FORMAT(CONVERT(DECIMAL(10, 2), [DiamReading]), 'N2')) AS [DiamReading]
        ,CONVERT(NVARCHAR(50), FORMAT(CONVERT(DECIMAL(10, 3), [RangeReading]), 'N3')) AS [RangeReading]
        ,CONVERT(NVARCHAR(50), FORMAT(CONVERT(DECIMAL(10, 2), [MaxOver]), 'N2')) AS [MaxOver]
        ,CONVERT(NVARCHAR(50), FORMAT(CONVERT(DECIMAL(10, 2), [MinUnder]), 'N2')) AS [MinUnder]
        ,CONVERT(NVARCHAR(50), FORMAT(CONVERT(DECIMAL(10, 2), [DiamUL]), 'N2')) AS [DiamUL]
        ,CONVERT(NVARCHAR(50), FORMAT(CONVERT(DECIMAL(10, 2), [DiamLL]), 'N2')) AS [DiamLL]
        ,[MaxOverUL]
        ,[MinUnderLL]
        ,[PassFail]
        ,[BuildType]
        ,[ShaftType]
    FROM [DatalogZMike]
    WHERE [DateTime] >= @DateFrom
        AND [DateTime] < @DateTo
		AND (@Part is null or [Part] like '%' + @Part + '%')
		AND (@StationName is null or [Station] like '%' + @StationName + '%')
		AND [ShaftType] = @ShaftType
	order by [WorkOrder], [Part], [ShaftNum]
	OFFSET     @skip ROWS  
	FETCH NEXT @Take ROWS ONLY; 
END;
GO
/****** Object:  StoredProcedure [dbo].[SearchZMikeReportCount]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SearchZMikeReportCount]
    @DateFrom DATE,
    @DateTo DATE,
    @Part NVARCHAR(50),
    @StationName NVARCHAR(50),
    @ShaftType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT count (1) 
    FROM [DatalogZMike]
    WHERE [DateTime] >= @DateFrom
        AND [DateTime] < @DateTo
		AND (@Part is null or [Part] like '%' + @Part + '%')
		AND (@StationName is null or [Station] like '%' + @StationName + '%')
		AND [ShaftType] = @ShaftType
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_GetFullPartInfo]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetFullPartInfo] 
	-- Add the parameters for the stored procedure here
	@partNum nvarchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT tip.Id,
		tip.ItemNumber,
		tip.FreqTarget,
		tip.DiamLL,
		tip.DiamUL,
		(SUBSTRING(tip.TipOdLength,10, len(tip.TipOdLength)-11)) TipOdLength, --tip.TipOdLength,
		tip.FormulaGId,
		tip.FormulaPoId,
		fG.U GU,
		fG.V GV,
		fG.X GX,
		fG.Y GY,
		fG.Z GZ,
		fG.P GP,
		fPo.U PoU,
		fPo.V PoV,
		fPo.X PoX,
		fPo.Y PoY,
		fPo.Z PoZ,
		fPo.P PoP
	FROM tblTipOdFreq tip 
		LEFT JOIN tblFormulaG fG on fG.Id = tip.FormulaGId
		LEFT JOIN tblFormulaPo fPo on fPo.Id = tip.FormulaGId
	WHERE tip.ItemNumber = @partNum
	ORDER BY CAST((SUBSTRING(tip.TipOdLength,10, len(tip.TipOdLength)-11)) AS int) asc
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetParts]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
Create PROCEDURE [dbo].[sp_GetParts]
	-- Add the parameters for the stored procedure here
	@part nvarchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	if @part = 'All'
		SELECT * from tblTipOdFreq
	else
		SELECT * from tblTipOdFreq where ItemNumber like '%'+@part+'%'
END
GO
/****** Object:  StoredProcedure [dbo].[sp_tblDataLogPolishingInsert]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_tblDataLogPolishingInsert]
	-- Add the parameters for the stored procedure here
	@shaftNum int,
	@partNum nvarchar(100),
	@workOrder nvarchar(100),
	@freqReading float,
	@freqTarget float,
	@motorPolishing float,
	@formulaPO int,
	@logType nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	INSERT INTO [dbo].[tblDataLogPolishing]
           ([ShaftNumber]
           ,[Part]
           ,[WorkOrder]
           ,[FreqReading]
           ,[FreqTarget]
           ,[MortorPolishing]
           ,[FormulaPO]
           ,[LogType])
     VALUES
           (
		    @shaftNum,
			@partNum,
			@workOrder,
			@freqReading,
			@freqTarget,
			@motorPolishing,
			@formulaPO,
			@logType
		   )
END
GO
/****** Object:  StoredProcedure [dbo].[sp_tblDataLogSandingInsert]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_tblDataLogSandingInsert]
	-- Add the parameters for the stored procedure here
	@shaftNum int,
	@workOrder nvarchar(100),
	@partNum nvarchar(100),
	@freq01Reading float,
	@motorSandingSpeed float,
	@freq02Reading float,
	@freqTarget float,
	@formulaG int,
	@logType nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.	

    -- Insert statements for procedure here
	INSERT INTO [dbo].[tblDataLogSanding]
           (
           [ShaftNumber]
           ,[WorkOrder]
           ,[Part]
           ,[Freq01Reading]
           ,[MotorSandingSpeed]
           ,[Freq02Reading]
           ,[FreqTarget]
           ,[FormulaGId]
           ,[LogStyle])
     VALUES
           (
			@shaftNum,
			@workOrder,
			@partNum,
			@freq01Reading,
			@motorSandingSpeed,
			@freq02Reading,
			@freqTarget,
			@formulaG,
			@logType
		   )
END
GO
/****** Object:  StoredProcedure [dbo].[sp_tblDataLogTipOdInsert]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_tblDataLogTipOdInsert]
	-- Add the parameters for the stored procedure here
	@shaftNum int,
	@partNum nvarchar(100),
	@workOrder nvarchar(100),
	@diamReading float,
	@measType nvarchar(100),
	@diamLL float,
	@diamUL float,
	@passFail nvarchar(20),
	@logType nvarchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	INSERT INTO [dbo].[tblDataLogTipOD]
           (
			   [ShaftNumber]
			   ,[Part]
			   ,[WorkOrder]
			   ,[DiamReading]
			   ,[MeasType]
			   ,[DiamLL]
			   ,[DiamUL]
			   ,[PassFail]
			   ,[LogType]
		   )
     VALUES
           (
			   @shaftNum,
				@partNum,
				@workOrder,
				@diamReading,
				@measType,
				@diamLL,
				@diamUL,
				@passFail,
				@logType
		   )
END
GO
/****** Object:  StoredProcedure [dbo].[sp_tblFormulaGInsert]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_tblFormulaGInsert]
	-- Add the parameters for the stored procedure here
	@id int,
	@u int,
	@v int,
	@x float,
	@y float,
	@z int,
	@p float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	INSERT INTO [dbo].[tblFormulaG]
           ([Id]
           ,[U]
           ,[V]
           ,[X]
           ,[Y]
           ,[Z]
           ,[P])
     VALUES
           (@id
           ,@u
           ,@v
           ,@x
           ,@y
           ,@z
           ,@p
		   )
END
GO
/****** Object:  StoredProcedure [dbo].[sp_tblFormulaPoInsert]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_tblFormulaPoInsert]
	-- Add the parameters for the stored procedure here
	@id int,
	@u int,
	@v int,
	@x float,
	@y float,
	@z int,
	@p float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	INSERT INTO [dbo].[tblFormulaPo]
           ([Id]
           ,[U]
           ,[V]
           ,[X]
           ,[Y]
           ,[Z]
           ,[P])
     VALUES
           (@id
           ,@u
           ,@v
           ,@x
           ,@y
           ,@z
           ,@p
		   )
END
GO
/****** Object:  StoredProcedure [dbo].[sp_TipOdFreqInsert]    Script Date: 5/11/2026 3:53:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_TipOdFreqInsert]
	-- Add the parameters for the stored procedure here
	@itemNum nvarchar(100),
	@freq int,
	@diamLL float,
	@diamUL float,
	@tipOdLength nvarchar(500),
	@formulaGId int,
	@formulaPoId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	INSERT INTO [dbo].[tblTipOdFreq]
           (
           [ItemNumber]
           ,[FreqTarget]
           ,[DiamLL]
           ,[DiamUL]
           ,[TipOdLength]
           ,[FormulaGId]
           ,[FormulaPoId])
     VALUES
           (
           @itemNum
           ,@freq
           ,@diamLL
           ,@diamUl
           ,@tipOdLength
           ,@formulaGId
           ,@formulaPoId
		   )
END
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Kiểu log data. production: chỉ log 5 cây; Pilot: log toàn bộ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblDataLogSanding', @level2type=N'COLUMN',@level2name=N'LogStyle'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'part number, quét barcode mã này để query lấy data truyền xuống PLC chạy' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'tblTipOdFreq', @level2type=N'COLUMN',@level2name=N'ItemNumber'
GO
USE [master]
GO
ALTER DATABASE [ALD_MFG1] SET  READ_WRITE 
GO
