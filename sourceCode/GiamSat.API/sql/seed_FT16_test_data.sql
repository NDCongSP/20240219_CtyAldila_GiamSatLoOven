-- ============================================================
-- Seed dữ liệu TEST vào bảng FT16 cho Part + Work cụ thể
-- Mục đích: test endpoint /api/FT14/calcdata → StiffnessY(=SpineB)
-- SandingMode = 2 (EnumSandingMode.Test)
-- Chạy trên DB: oven (main DB — phucthinhautomation.ddns.net,1433)
-- ============================================================
-- Xóa data test cũ nếu muốn chạy lại sạch:
-- DELETE FROM FT16 WHERE Part = 'AX181-IS-1' AND Work = '1794406' AND SandingMode = 2;

USE oven;
GO

INSERT INTO FT16 (Id, CreatedAt, CreatedMachine, Part, Work, SandingMode, ShaftNum, SpineB, SpineTarget, Spine_Low, Spine_Hight)
VALUES
-- ShaftNum 1-10 với SpineB là giá trị Stiffness Y mẫu (đơn vị Kg)
-- Mỗi cặp shaft có cùng tốc độ motor (khớp với logic 2 row/bước nhảy RPM)
(NEWID(), GETDATE(), 'SEED_SCRIPT', 'AX181-IS-1', '1794406', 2,  1, 2.546, 2.55, 2.45, 2.65),
(NEWID(), GETDATE(), 'SEED_SCRIPT', 'AX181-IS-1', '1794406', 2,  2, 2.552, 2.55, 2.45, 2.65),
(NEWID(), GETDATE(), 'SEED_SCRIPT', 'AX181-IS-1', '1794406', 2,  3, 2.526, 2.55, 2.45, 2.65),
(NEWID(), GETDATE(), 'SEED_SCRIPT', 'AX181-IS-1', '1794406', 2,  4, 2.526, 2.55, 2.45, 2.65),
(NEWID(), GETDATE(), 'SEED_SCRIPT', 'AX181-IS-1', '1794406', 2,  5, 2.436, 2.55, 2.45, 2.65),
(NEWID(), GETDATE(), 'SEED_SCRIPT', 'AX181-IS-1', '1794406', 2,  6, 2.445, 2.55, 2.45, 2.65),
(NEWID(), GETDATE(), 'SEED_SCRIPT', 'AX181-IS-1', '1794406', 2,  7, 2.398, 2.55, 2.45, 2.65),
(NEWID(), GETDATE(), 'SEED_SCRIPT', 'AX181-IS-1', '1794406', 2,  8, 2.366, 2.55, 2.45, 2.65),
(NEWID(), GETDATE(), 'SEED_SCRIPT', 'AX181-IS-1', '1794406', 2,  9, 2.372, 2.55, 2.45, 2.65),
(NEWID(), GETDATE(), 'SEED_SCRIPT', 'AX181-IS-1', '1794406', 2, 10, 2.364, 2.55, 2.45, 2.65);
GO

-- Verify
SELECT ShaftNum, SpineB, SandingMode, CreatedAt
FROM FT16
WHERE Part = 'AX181-IS-1' AND Work = '1794406' AND SandingMode = 2
ORDER BY ShaftNum;
GO
