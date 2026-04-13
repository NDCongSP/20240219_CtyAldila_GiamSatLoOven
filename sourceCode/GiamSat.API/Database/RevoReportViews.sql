/*
  Báo cáo REVO — 3 chế độ (Theo step / Theo shaft / Theo giờ) dưới dạng VIEW.
  Nguồn: bảng dbo.FT09 (FT09_RevoDatalog).

  Chạy script trên SQL Server (SSMS / sqlcmd). Điều chỉnh schema nếu bảng không nằm dbo.

  Lưu ý:
  - vw_RevoReport_Step: ShaftNo / ShaftKey tính trên toàn bộ bảng FT09. Để khớp RevoReport.razor.cs (map shaft
    chỉ trong tập đã lọc ngày + RevoId), dùng dbo.fn_RevoReport_Step (TVF có tham số).
  - vw_RevoReport_Shaft / vw_RevoReport_Hour: cùng hạn chế khi cần lọc — ưu tiên fn_RevoReport_Shaft, fn_RevoReport_Hour.
  - IsAutoRolling khớp RevoReport.razor.cs (chuỗi "auto rolling" / "autorolling").
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.vw_RevoReport_Base', N'V') IS NOT NULL
    DROP VIEW dbo.vw_RevoReport_Base;
GO

CREATE VIEW dbo.vw_RevoReport_Base
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

/* Helper: giây -> chuỗi HH:mm:ss (2 chữ số giờ như C# (int)TotalHours — dùng floor giờ tổng) */
IF OBJECT_ID(N'dbo.fn_Revo_SecondsToHhMmSs', N'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_Revo_SecondsToHhMmSs;
GO

CREATE FUNCTION dbo.fn_Revo_SecondsToHhMmSs (@seconds INT)
RETURNS NVARCHAR(16)
AS
BEGIN
    IF @seconds IS NULL OR @seconds < 0 SET @seconds = 0;
    DECLARE @h INT = @seconds / 3600;
    DECLARE @m INT = (@seconds % 3600) / 60;
    DECLARE @s INT = @seconds % 60;
    RETURN CONCAT(
        RIGHT(CONCAT(N'0', @h), 2), N':',
        RIGHT(CONCAT(N'0', @m), 2), N':',
        RIGHT(CONCAT(N'0', @s), 2)
    );
END;
GO

IF OBJECT_ID(N'dbo.vw_RevoReport_Step', N'V') IS NOT NULL
    DROP VIEW dbo.vw_RevoReport_Step;
GO

CREATE VIEW dbo.vw_RevoReport_Step
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

IF OBJECT_ID(N'dbo.vw_RevoReport_Shaft', N'V') IS NOT NULL
    DROP VIEW dbo.vw_RevoReport_Shaft;
GO

CREATE VIEW dbo.vw_RevoReport_Shaft
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

IF OBJECT_ID(N'dbo.vw_RevoReport_Hour', N'V') IS NOT NULL
    DROP VIEW dbo.vw_RevoReport_Hour;
GO

CREATE VIEW dbo.vw_RevoReport_Hour
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

/* --- TVF: lọc Started + RevoId, map shaft global đúng phạm vi báo cáo (khớp UI) --- */

IF OBJECT_ID(N'dbo.fn_RevoReport_Step', N'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_RevoReport_Step;
GO

CREATE FUNCTION dbo.fn_RevoReport_Step (
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

IF OBJECT_ID(N'dbo.fn_RevoReport_Shaft', N'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_RevoReport_Shaft;
GO

CREATE FUNCTION dbo.fn_RevoReport_Shaft (
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

IF OBJECT_ID(N'dbo.fn_RevoReport_Hour', N'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_RevoReport_Hour;
GO

CREATE FUNCTION dbo.fn_RevoReport_Hour (
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

/*
  Tích hợp API / EF Core (gợi ý):
  - SELECT * FROM dbo.fn_RevoReport_Step(@from, @to, @revoId, @shaftScope);  @shaftScope = N'total' | N'finished'
  - FromSqlRaw: "SELECT * FROM dbo.fn_RevoReport_Step({0},{1},{2},{3})", ... tham số 4: total/finished
  - Keyless entity: modelBuilder.Entity<RevoReportStepRowVm>().HasNoKey();
*/
