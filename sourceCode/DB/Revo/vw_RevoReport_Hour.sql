USE [oven]
GO

/****** Object:  View [dbo].[vw_RevoReport_Hour]    Script Date: 14/04/2026 10:05:17 SA ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vw_RevoReport_Hour]
AS
WITH g AS (
    SELECT
        b.HourBucket,
        b.IsAutoRolling,
        b.ShaftNum,
        b.TotalTime,
        b.StartedAt,
        b.EndedAt,
        b.Started
    FROM dbo.vw_RevoReport_Base AS b
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
    StartedAt = CASE WHEN a.NonAutoCnt = 0 AND a.AutoCnt > 0 THEN NULL
        ELSE COALESCE(a.MinStartedNonAuto, a.MinStartedFallback) END,
    EndedAt = CASE WHEN a.NonAutoCnt = 0 AND a.AutoCnt > 0 THEN NULL
        ELSE a.MaxEndedNonAuto END,
    TotalTimeSeconds = a.TotalSeconds,
    TotalTimeText = CASE WHEN a.TotalSeconds > 0
        THEN dbo.fn_Revo_SecondsToHhMmSs(CAST(ROUND(a.TotalSeconds, 0) AS INT))
        ELSE N'00:00:00' END
FROM agg AS a;
GO

