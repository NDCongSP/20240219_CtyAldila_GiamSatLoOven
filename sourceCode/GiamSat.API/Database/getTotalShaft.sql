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

    -- Lấy thông tin tổng hợp của từng Shaft (xét toàn bộ quá trình của shaft)
    WITH ShaftStats AS (
        SELECT 
            RevoId,
            ShaftNum,
            MIN(CreatedAt) AS StartedAt,
            MAX(CreatedAt) AS LastActivityAt,
            CASE WHEN COUNT(*) = COUNT(CASE WHEN TotalTime > 0 THEN 1 END) THEN 1 ELSE 0 END AS IsFinished
        FROM FT09
        WHERE ShaftNum IS NOT NULL
          AND (@RevoId IS NULL OR RevoId = @RevoId)
          -- Tối ưu: chỉ xét dữ liệu trong vòng 48h qua để tránh full table scan
          AND CreatedAt >= DATEADD(HOUR, -48, @lastHour)
        GROUP BY RevoId, ShaftNum
    ),
    HourStats AS (
        SELECT 
            t.RevoId,
            -- Shaft được coi là Total của giờ nếu có bất kỳ activity nào trong giờ đó
            COUNT(DISTINCT CASE WHEN t.CreatedAt >= @currentHour AND t.CreatedAt < @nextHour THEN t.ShaftNum END) AS TotalShaftCurrentHour,
            COUNT(DISTINCT CASE WHEN t.CreatedAt >= @lastHour AND t.CreatedAt < @currentHour THEN t.ShaftNum END) AS TotalShaftLastHour
        FROM FT09 t
        WHERE t.ShaftNum IS NOT NULL
          AND (@RevoId IS NULL OR t.RevoId = @RevoId)
          AND t.CreatedAt >= @lastHour AND t.CreatedAt < @nextHour 
        GROUP BY t.RevoId
    ),
    FinishedStats AS (
        SELECT 
            RevoId,
            -- Shaft hoàn thành được tính vào giờ của LastActivityAt
            SUM(CASE WHEN IsFinished = 1 AND LastActivityAt >= @currentHour AND LastActivityAt < @nextHour THEN 1 ELSE 0 END) AS TotalShaftFinishCurrentHour,
            SUM(CASE WHEN IsFinished = 1 AND LastActivityAt >= @lastHour AND LastActivityAt < @currentHour THEN 1 ELSE 0 END) AS TotalShaftFinshLastHour
        FROM ShaftStats
        GROUP BY RevoId
    )

    SELECT 
        h.RevoId,
        ISNULL(h.TotalShaftCurrentHour, 0) AS TotalShaftCurrentHour,
        ISNULL(f.TotalShaftFinishCurrentHour, 0) AS TotalShaftFinishCurrentHour,
        ISNULL(h.TotalShaftLastHour, 0) AS TotalShaftLastHour,
        ISNULL(f.TotalShaftFinshLastHour, 0) AS TotalShaftFinshLastHour
    FROM HourStats h
    LEFT JOIN FinishedStats f ON h.RevoId = f.RevoId;

END