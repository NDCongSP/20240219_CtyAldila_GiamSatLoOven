USE [oven]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_RevoReport_Step]    Script Date: 14/04/2026 10:40:36 SA ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[fn_RevoReport_Step] (
    @FromDate DATETIME2(7),
    @ToDate DATETIME2(7),
    @RevoId INT NULL,
    @ShaftScope NVARCHAR(12) = N'total'
)
RETURNS TABLE
AS
RETURN
(
    WITH filtered AS (
        SELECT b.*
        FROM dbo.vw_RevoReport_Base AS b
        WHERE b.Started >= @FromDate
          AND b.Started < @ToDate
          AND (@RevoId IS NULL OR b.RevoId = @RevoId)
    ),
    shaft_finish AS (
        SELECT f.ShaftNum
        FROM filtered AS f
        WHERE f.ShaftNum IS NOT NULL
        GROUP BY f.ShaftNum
        HAVING COUNT_BIG(*) > 0
           AND COUNT_BIG(*) = COUNT_BIG(CASE WHEN ISNULL(f.TotalTime, 0) > 0 THEN 1 END)
    ),
    shaft_first AS (
        SELECT f.ShaftNum, MIN(f.Started) AS MinStarted
        FROM filtered AS f
        WHERE f.ShaftNum IS NOT NULL
        GROUP BY f.ShaftNum
    ),
    shaft_rank AS (
        SELECT
            ShaftNum,
            GlobalShaftNo = ROW_NUMBER() OVER (ORDER BY MinStarted)
        FROM shaft_first
    )
    SELECT
        b.Id,
        b.RevoId,
        b.RevoName,
        b.HourBucket AS [Hour],
        HourBucketDisplay = CONCAT(
            FORMAT(b.HourBucket, N'HH'), N':00-',
            FORMAT(DATEADD(HOUR, 1, b.HourBucket), N'HH'), N':00'
        ),
        ShaftNo = ISNULL(sr.GlobalShaftNo, 0),
        ShaftKey = CASE
            WHEN ISNULL(sr.GlobalShaftNo, 0) > 0 THEN CONCAT(N'Shaft ', sr.GlobalShaftNo)
            ELSE N'(Không xác định)'
        END,
        b.ShaftNum,
        b.Part,
        b.Work,
        b.Rev,
        b.Mandrel,
        StepDisplay = CONCAT(
            CASE WHEN NULLIF(LTRIM(RTRIM(b.StepName)), N'') IS NULL THEN N'N/A' ELSE b.StepName END,
            N' (',
            CASE WHEN b.StepId IS NULL THEN N'N/A' ELSE CAST(b.StepId AS NVARCHAR(20)) END,
            N')'
        ),
        DisplayStartedAt = CASE WHEN b.IsAutoRolling = 1 THEN NULL ELSE b.StartedAt END,
        DisplayEndedAt = CASE WHEN b.IsAutoRolling = 1 THEN NULL ELSE b.EndedAt END,
        DurationText = CASE
            WHEN b.IsAutoRolling = 1 THEN
                CASE WHEN ISNULL(b.TotalTime, 0) > 0
                    THEN dbo.fn_Revo_SecondsToHhMmSs(CAST(ROUND(b.TotalTime, 0) AS INT))
                    ELSE N'N/A' END
            WHEN b.StartedAt IS NOT NULL AND b.EndedAt IS NOT NULL THEN
                dbo.fn_Revo_SecondsToHhMmSs(DATEDIFF(SECOND, b.StartedAt, b.EndedAt))
            WHEN b.StartedAt IS NOT NULL AND b.EndedAt IS NULL THEN N'Đang chạy...'
            ELSE N'N/A'
        END,
        b.IsAutoRolling,
        b.Started,
        Stt = ROW_NUMBER() OVER (
            PARTITION BY ISNULL(sr.GlobalShaftNo, 0)
            ORDER BY b.Started
        ),
        IsShaftFinished = CAST(CASE WHEN b.ShaftNum IS NULL THEN NULL WHEN sf.ShaftNum IS NOT NULL THEN 1 ELSE 0 END AS BIT),
        HighlightIncomplete = CAST(CASE
            WHEN @ShaftScope = N'total' AND b.ShaftNum IS NOT NULL AND sf.ShaftNum IS NULL THEN 1 ELSE 0 END AS BIT)
    FROM filtered AS b
    LEFT JOIN shaft_rank AS sr ON b.ShaftNum = sr.ShaftNum
    LEFT JOIN shaft_finish AS sf ON b.ShaftNum = sf.ShaftNum
    WHERE @ShaftScope <> N'finished' OR b.ShaftNum IS NULL OR sf.ShaftNum IS NOT NULL
);
GO

