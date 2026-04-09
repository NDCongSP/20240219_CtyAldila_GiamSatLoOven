USE [oven]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetTotalShaft]    Script Date: 09/04/2026 9:43:47 CH ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetTotalShaft]
    @RevoId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @maxCreatedAt DATETIME;
    DECLARE @currentHour DATETIME;
    DECLARE @nextHour DATETIME;
    DECLARE @lastHour DATETIME;

    -- 1. max time
    SELECT @maxCreatedAt = MAX(CreatedAt)
    FROM FT09
    WHERE CreatedAt IS NOT NULL
      AND (@RevoId IS NULL OR RevoId = @RevoId);

    IF @maxCreatedAt IS NULL
    BEGIN
        SELECT 0 RevoId, 0 TotalShaftCurrentHour, 0 TotalShaftFinishCurrentHour, 0 TotalShaftLastHour, 0 TotalShaftFinshLastHour;
        RETURN;
    END

    -- 2. time window
    SET @currentHour = DATEADD(HOUR, DATEDIFF(HOUR, 0, @maxCreatedAt), 0);
    SET @nextHour = DATEADD(HOUR, 1, @currentHour);
    SET @lastHour = DATEADD(HOUR, -1, @currentHour);

    -- 3. CTE: Shaft hoàn thành giờ hiện tại
    WITH FinishedCurrent AS (
        SELECT RevoId, ShaftNum
        FROM FT09
        WHERE CreatedAt >= @currentHour AND CreatedAt < @nextHour
          AND ShaftNum IS NOT NULL
          AND (@RevoId IS NULL OR RevoId = @RevoId)
        GROUP BY RevoId, ShaftNum
        HAVING COUNT(*) = COUNT(CASE WHEN TotalTime > 0 THEN 1 END)
    ),

    -- 4. CTE: Shaft hoàn thành giờ trước
    FinishedLast AS (
        SELECT RevoId, ShaftNum
        FROM FT09
        WHERE CreatedAt >= @lastHour AND CreatedAt < @currentHour
          AND ShaftNum IS NOT NULL
          AND (@RevoId IS NULL OR RevoId = @RevoId)
        GROUP BY RevoId, ShaftNum
        HAVING COUNT(*) = COUNT(CASE WHEN TotalTime > 0 THEN 1 END)
    )

    -- 5. Query chính
    SELECT 
        t.RevoId,

        -- Tổng shaft giờ hiện tại
        COUNT(DISTINCT CASE 
            WHEN t.CreatedAt >= @currentHour AND t.CreatedAt < @nextHour 
            THEN t.ShaftNum END) AS TotalShaftCurrentHour,

        -- Shaft hoàn thành giờ hiện tại
        COUNT(DISTINCT fc.ShaftNum) AS TotalShaftFinishCurrentHour,

        -- Tổng shaft giờ trước
        COUNT(DISTINCT CASE 
            WHEN t.CreatedAt >= @lastHour AND t.CreatedAt < @currentHour 
            THEN t.ShaftNum END) AS TotalShaftLastHour,

        -- Shaft hoàn thành giờ trước
        COUNT(DISTINCT fl.ShaftNum) AS TotalShaftFinshLastHour

    FROM FT09 t
    LEFT JOIN FinishedCurrent fc 
        ON t.RevoId = fc.RevoId AND t.ShaftNum = fc.ShaftNum

    LEFT JOIN FinishedLast fl 
        ON t.RevoId = fl.RevoId AND t.ShaftNum = fl.ShaftNum

    WHERE t.ShaftNum IS NOT NULL
      AND (@RevoId IS NULL OR t.RevoId = @RevoId)

    GROUP BY t.RevoId
    ORDER BY t.RevoId;

END
GO

