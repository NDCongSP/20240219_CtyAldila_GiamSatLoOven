USE [oven]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_RevoReport_Shaft]    Script Date: 14/04/2026 10:40:26 SA ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[fn_RevoReport_Shaft] (
    @FromDate DATETIME2(7),
    @ToDate DATETIME2(7),
    @RevoId INT NULL,
    @ShaftScope NVARCHAR(12) = N'total'
)
RETURNS TABLE
AS
RETURN
(
    WITH base AS (
        SELECT b.*
        FROM dbo.vw_RevoReport_Base AS b
        WHERE b.Started >= @FromDate
          AND b.Started < @ToDate
          AND (@RevoId IS NULL OR b.RevoId = @RevoId)
          AND b.ShaftNum IS NOT NULL
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
            RevoId = MAX(CASE WHEN rn = 1 THEN b.RevoId END),
            RevoName = MAX(CASE WHEN rn = 1 THEN b.RevoName END),
            Part = MAX(CASE WHEN rn = 1 THEN b.Part END),
            Work = MAX(CASE WHEN rn = 1 THEN b.Work END),
            Mandrel = MAX(CASE WHEN rn = 1 THEN b.Mandrel END),
            IsShaftFinished = CAST(CASE
                WHEN COUNT_BIG(*) = COUNT_BIG(CASE WHEN ISNULL(b.TotalTime, 0) > 0 THEN 1 END) AND COUNT_BIG(*) > 0 THEN 1
                ELSE 0 END AS BIT)
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
        TotalTimeText = CASE WHEN o.TotalSeconds > 0
            THEN dbo.fn_Revo_SecondsToHhMmSs(CAST(ROUND(o.TotalSeconds, 0) AS INT))
            ELSE N'00:00:00' END,
        o.StepCount,
        o.HasAutoRolling AS IsAutoRollingShaft,
        o.IsShaftFinished,
        HighlightIncomplete = CAST(CASE WHEN @ShaftScope = N'total' AND o.IsShaftFinished = 0 THEN 1 ELSE 0 END AS BIT)
    FROM ordered AS o
    WHERE @ShaftScope <> N'finished' OR o.IsShaftFinished = 1
);
GO

