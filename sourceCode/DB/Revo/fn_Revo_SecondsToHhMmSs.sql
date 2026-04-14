USE [oven]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_Revo_SecondsToHhMmSs]    Script Date: 14/04/2026 10:39:52 SA ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[fn_Revo_SecondsToHhMmSs] (@seconds INT)
RETURNS NVARCHAR(16)
AS
BEGIN
    IF @seconds IS NULL OR @seconds < 0 SET @seconds = 0;
    DECLARE @h INT = @seconds / 3600;
    DECLARE @m INT = (@seconds % 3600) / 60;
    DECLARE @s INT = @seconds % 60;
    RETURN CONCAT(
        RIGHT(CONCAT(N'0', @h), 2), N':',
        RIGHT(CONCAT(N'0', @m), 2), N':',
        RIGHT(CONCAT(N'0', @s), 2)
    );
END;
GO

