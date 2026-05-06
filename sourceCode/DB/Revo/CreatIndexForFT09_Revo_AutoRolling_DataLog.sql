--select * from ft08

--SELECT *
--  FROM [oven].[dbo].[FT09]
--  where CreatedAt >='2026-05-02 00:00:00'
--  order by CreatedAt desc

--  --exec sp_GetTotalShaft

--  --delete [oven].[dbo].[FT09]  where CreatedAt >='2026-05-02 00:00:00'

-- Index chính cho bộ key truy vấn
CREATE NONCLUSTERED INDEX IX_FT09_ShaftNum_RevoId_StepId
ON FT09 (ShaftNum, RevoId, StepId)
INCLUDE (StartedAt, EndedAt, TotalTime, StepName);

-- Index phụ nếu cần lọc theo thời gian
CREATE NONCLUSTERED INDEX IX_FT09_CreatedAt
ON FT09 (CreatedAt);