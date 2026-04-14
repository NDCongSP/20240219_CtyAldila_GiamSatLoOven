USE [oven]
GO

/****** Object:  View [dbo].[vw_RevoReport_Shaft]    Script Date: 14/04/2026 10:05:28 SA ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vw_RevoReport_Shaft]
AS
WITH base AS (
    SELECT * FROM dbo.vw_RevoReport_Base WHERE ShaftNum IS NOT NULL
),
per_shaft AS (
    SELECT
        b.ShaftNum,
        HasAutoRolling = MAX(b.IsAutoRolling),
        TotalSeconds = SUM(ISNULL(b.TotalTime, 0)),
        StepCount = COUNT_BIG(*),
        MinStarted = MIN(b.Started),
        MinStartedAt = MIN(b.StartedAt),
        MaxEndedAt = MAX(b.EndedAt),
        /* Hàng đầu theo Started — Part, Work, Mandrel, RevoName */
        RevoId = MAX(CASE WHEN rn = 1 THEN b.RevoId END),
        RevoName = MAX(CASE WHEN rn = 1 THEN b.RevoName END),
        Part = MAX(CASE WHEN rn = 1 THEN b.Part END),
        Work = MAX(CASE WHEN rn = 1 THEN b.Work END),
        Mandrel = MAX(CASE WHEN rn = 1 THEN b.Mandrel END)
    FROM (
        SELECT *,
            rn = ROW_NUMBER() OVER (PARTITION BY ShaftNum ORDER BY Started)
        FROM base
    ) AS b
    GROUP BY b.ShaftNum
),
ordered AS (
    SELECT
        p.*,
        OrderKey = CASE
            WHEN p.HasAutoRolling = 1 THEN p.MinStarted
            ELSE COALESCE(p.MinStartedAt, p.MinStarted)
        END
    FROM per_shaft AS p
)
SELECT
    ShaftLabel = CONCAT(N'Shaft ', ROW_NUMBER() OVER (ORDER BY o.OrderKey, o.ShaftNum)),
    o.RevoId,
    o.RevoName,
    [Hour] = DATEADD(HOUR, DATEDIFF(HOUR, 0, COALESCE(o.MinStartedAt, o.MinStarted)), 0),
    HourBucket = CONCAT(
        FORMAT(DATEADD(HOUR, DATEDIFF(HOUR, 0, COALESCE(o.MinStartedAt, o.MinStarted)), 0), N'HH'), N':00-',
        FORMAT(DATEADD(HOUR, 1 + DATEDIFF(HOUR, 0, COALESCE(o.MinStartedAt, o.MinStarted)), 0), N'HH'), N':00'
    ),
    ShaftNo = ROW_NUMBER() OVER (ORDER BY o.OrderKey, o.ShaftNum),
    Stt = ROW_NUMBER() OVER (ORDER BY o.OrderKey, o.ShaftNum),
    o.ShaftNum,
    o.Part,
    o.Work,
    o.Mandrel,
    StartedAt = CASE WHEN o.HasAutoRolling = 1 THEN NULL ELSE o.MinStartedAt END,
    EndedAt = CASE WHEN o.HasAutoRolling = 1 THEN NULL ELSE o.MaxEndedAt END,
    TotalTimeSeconds = o.TotalSeconds,
    TotalTimeText = CASE WHEN o.TotalSeconds > 0 THEN dbo.fn_Revo_SecondsToHhMmSs(CAST(ROUND(o.TotalSeconds, 0) AS INT)) ELSE N'00:00:00' END,
    o.StepCount,
    o.HasAutoRolling AS IsAutoRollingShaft
FROM ordered AS o;
GO

