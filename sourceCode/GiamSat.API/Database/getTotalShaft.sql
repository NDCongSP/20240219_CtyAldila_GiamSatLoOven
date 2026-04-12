USE [oven]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetTotalShaft]    Script Date: 4/12/2026 10:23:35 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[sp_GetTotalShaft]
    @RevoId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @maxCreatedAt DATETIME;
    DECLARE @currentHour DATETIME;
    DECLARE @nextHour DATETIME;
    DECLARE @lastHour DATETIME;

    -- Lấy mốc thời gian mới nhất của TOÀN HỆ THỐNG
    SELECT @maxCreatedAt = MAX(CreatedAt) FROM FT09;

    IF @maxCreatedAt IS NULL RETURN;

    -- Thiết lập khung giờ cố định
    SET @currentHour = DATEADD(HOUR, DATEDIFF(HOUR, 0, @maxCreatedAt), 0);
    SET @nextHour = DATEADD(HOUR, 1, @currentHour);
    SET @lastHour = DATEADD(HOUR, -1, @currentHour);

    -- Các CTE giữ nguyên logic lọc @RevoId
    WITH FinishedCurrent AS (
        SELECT RevoId, ShaftNum
        FROM FT09
        WHERE CreatedAt >= @currentHour AND CreatedAt < @nextHour
          AND ShaftNum IS NOT NULL
          AND (@RevoId IS NULL OR RevoId = @RevoId)
        GROUP BY RevoId, ShaftNum
        HAVING COUNT(*) = COUNT(CASE WHEN TotalTime > 0 THEN 1 END)
    ),
    FinishedLast AS (
        SELECT RevoId, ShaftNum
        FROM FT09
        WHERE CreatedAt >= @lastHour AND CreatedAt < @currentHour
          AND ShaftNum IS NOT NULL
          AND (@RevoId IS NULL OR RevoId = @RevoId)
        GROUP BY RevoId, ShaftNum
        HAVING COUNT(*) = COUNT(CASE WHEN TotalTime > 0 THEN 1 END)
    )
    -- Query chính...
    SELECT 
        t.RevoId,
        COUNT(DISTINCT CASE WHEN t.CreatedAt >= @currentHour AND t.CreatedAt < @nextHour THEN t.ShaftNum END) AS TotalShaftCurrentHour,
        COUNT(DISTINCT fc.ShaftNum) AS TotalShaftFinishCurrentHour,
        COUNT(DISTINCT CASE WHEN t.CreatedAt >= @lastHour AND t.CreatedAt < @currentHour THEN t.ShaftNum END) AS TotalShaftLastHour,
        COUNT(DISTINCT fl.ShaftNum) AS TotalShaftFinshLastHour
    FROM FT09 t
    LEFT JOIN FinishedCurrent fc ON t.RevoId = fc.RevoId AND t.ShaftNum = fc.ShaftNum
    LEFT JOIN FinishedLast fl ON t.RevoId = fl.RevoId AND t.ShaftNum = fl.ShaftNum
    WHERE t.ShaftNum IS NOT NULL
      AND (@RevoId IS NULL OR t.RevoId = @RevoId)
      -- Quan trọng: Chỉ lấy dữ liệu trong 2 khung giờ đang xét để tránh quét toàn bộ bảng gây sai số
      AND t.CreatedAt >= @lastHour AND t.CreatedAt < @nextHour 
    GROUP BY t.RevoId;
END