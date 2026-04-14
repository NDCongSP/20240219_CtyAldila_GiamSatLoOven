USE [oven]
GO

/****** Object:  View [dbo].[vw_RevoReport_Base]    Script Date: 14/04/2026 10:05:03 SA ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[vw_RevoReport_Base]
AS
SELECT
    f.Id,
    f.CreatedAt,
    f.CreatedMachine,
    f.RevoId,
    f.RevoName,
    f.Work,
    f.Part,
    f.Rev,
    f.ColorCode,
    f.Mandrel,
    f.MandrelStart,
    f.StepId,
    f.StepName,
    f.StartedAt,
    f.EndedAt,
    f.ShaftNum,
    f.TotalTime,
    /* Normalized start — giống C# Started */
    [Started] = COALESCE(f.StartedAt, f.CreatedAt),
    /* Bucket giờ */
    [HourBucket] = DATEADD(HOUR, DATEDIFF(HOUR, 0, COALESCE(f.StartedAt, f.CreatedAt)), 0),
    /* Giống IsAutoRolling(FT09_RevoDatalog) trong RevoReport.razor.cs */
    [IsAutoRolling] = CASE
        WHEN CHARINDEX(N'auto rolling', LOWER(ISNULL(f.RevoName, N''))) > 0 THEN 1
        WHEN CHARINDEX(N'auto rolling', LOWER(ISNULL(f.Work, N''))) > 0 THEN 1
        WHEN CHARINDEX(N'autorolling', LOWER(REPLACE(ISNULL(f.RevoName, N''), N' ', N''))) > 0 THEN 1
        WHEN CHARINDEX(N'autorolling', LOWER(REPLACE(ISNULL(f.Work, N''), N' ', N''))) > 0 THEN 1
        ELSE 0
    END
FROM dbo.FT09 AS f
WHERE COALESCE(f.StartedAt, f.CreatedAt) IS NOT NULL;
GO

