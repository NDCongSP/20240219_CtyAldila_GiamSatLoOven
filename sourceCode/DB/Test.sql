SELECT TotalTime, *
  FROM [oven].[dbo].[FT09]
  where revoId = 2
  order by CreatedAt desc

EXEC sp_GetTotalShaft @RevoId =6

