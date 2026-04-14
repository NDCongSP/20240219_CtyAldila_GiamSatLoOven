USE [oven]
GO

/****** Object:  View [dbo].[vw_RevoReport_Step]    Script Date: 14/04/2026 10:05:45 SA ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vw_RevoReport_Step]
AS
WITH shaft_first AS (
    SELECT b.ShaftNum, MIN(b.Started) AS MinStarted
    FROM dbo.vw_RevoReport_Base AS b
    WHERE b.ShaftNum IS NOT NULL
    GROUP BY b.ShaftNum
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
    /* Auto rolling: ẩn StartedAt/EndedAt trên grid như C# */
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
    )
FROM dbo.vw_RevoReport_Base AS b
LEFT JOIN shaft_rank AS sr ON b.ShaftNum = sr.ShaftNum;
GO

