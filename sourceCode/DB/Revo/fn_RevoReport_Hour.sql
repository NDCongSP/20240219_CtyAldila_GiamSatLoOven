USE [oven]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_RevoReport_Hour]    Script Date: 14/04/2026 10:40:15 SA ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[fn_RevoReport_Hour] (
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
    /* Hoàn thành shaft: chỉ gán 1 lần vào giờ bắt đầu (MIN(Started)), tránh đếm trùng khi các step
       nằm ở hai khung giờ (StartedAt/EndedAt kéo dài qua giờ). Không tách logic auto rolling riêng. */
    shaft_first_hour AS (
        SELECT
            f.ShaftNum,
            FirstHourBucket = DATEADD(HOUR, DATEDIFF(HOUR, 0, MIN(f.Started)), 0)
        FROM filtered AS f
        WHERE f.ShaftNum IS NOT NULL
        GROUP BY f.ShaftNum
    ),
    g AS (
        SELECT
            b.HourBucket,
            b.IsAutoRolling,
            b.ShaftNum,
            b.TotalTime,
            b.StartedAt,
            b.EndedAt,
            b.Started
        FROM filtered AS b
        LEFT JOIN shaft_finish AS sf ON b.ShaftNum = sf.ShaftNum
        WHERE @ShaftScope <> N'finished' OR b.ShaftNum IS NULL OR sf.ShaftNum IS NOT NULL
    ),
    hour_shaft_finished AS (
        SELECT s.FirstHourBucket AS HourBucket, s.ShaftNum
        FROM shaft_first_hour AS s
        INNER JOIN shaft_finish AS sf ON sf.ShaftNum = s.ShaftNum
    ),
    agg AS (
        SELECT
            HourBucket,
            NonAutoCnt = SUM(CASE WHEN IsAutoRolling = 0 THEN 1 ELSE 0 END),
            AutoCnt = SUM(CASE WHEN IsAutoRolling = 1 THEN 1 ELSE 0 END),
            TotalSeconds = SUM(ISNULL(TotalTime, 0)),
            ShaftCount = COUNT(DISTINCT CASE WHEN ShaftNum IS NOT NULL THEN ShaftNum END),
            MinStartedNonAuto = MIN(CASE WHEN IsAutoRolling = 0 THEN StartedAt END),
            MaxEndedNonAuto = MAX(CASE WHEN IsAutoRolling = 0 THEN EndedAt END),
            MinStartedFallback = MIN(Started)
        FROM g
        GROUP BY HourBucket
    )
    SELECT
        [Hour] = a.HourBucket,
        HourRange = CONCAT(
            FORMAT(a.HourBucket, N'dd/MM/yyyy HH'), N':00-',
            FORMAT(DATEADD(HOUR, 1, a.HourBucket), N'HH'), N':00'
        ),
        a.ShaftCount,
        ShaftCountFinishedInHour = ISNULL((
            SELECT COUNT_BIG(DISTINCT h.ShaftNum) FROM hour_shaft_finished h WHERE h.HourBucket = a.HourBucket), 0),
        IncompleteShaftCountInHour = ISNULL((
            SELECT COUNT_BIG(DISTINCT x.ShaftNum)
            FROM g x
            WHERE x.HourBucket = a.HourBucket
              AND x.ShaftNum IS NOT NULL
              AND NOT EXISTS (SELECT 1 FROM shaft_finish sf WHERE sf.ShaftNum = x.ShaftNum)
        ), 0),
        HighlightIncomplete = CAST(CASE
            WHEN @ShaftScope = N'total' AND EXISTS (
                SELECT 1
                FROM g x
                WHERE x.HourBucket = a.HourBucket
                  AND x.ShaftNum IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM shaft_finish sf WHERE sf.ShaftNum = x.ShaftNum)
            ) THEN 1 ELSE 0 END AS BIT),
        StartedAt = CASE WHEN a.NonAutoCnt = 0 AND a.AutoCnt > 0 THEN NULL
            ELSE COALESCE(a.MinStartedNonAuto, a.MinStartedFallback) END,
        EndedAt = CASE WHEN a.NonAutoCnt = 0 AND a.AutoCnt > 0 THEN NULL
            ELSE a.MaxEndedNonAuto END,
        TotalTimeSeconds = a.TotalSeconds,
        TotalTimeText = CASE WHEN a.TotalSeconds > 0
            THEN dbo.fn_Revo_SecondsToHhMmSs(CAST(ROUND(a.TotalSeconds, 0) AS INT))
            ELSE N'00:00:00' END
    FROM agg AS a
);
GO

