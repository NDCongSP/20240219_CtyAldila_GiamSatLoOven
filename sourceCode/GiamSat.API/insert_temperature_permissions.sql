USE [YourDatabaseName] -- Thay bằng tên database của bạn
GO

DECLARE @CreatedAt DATETIME = GETDATE();

-- 1. Temperature_Home.View
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Module = 'Temperature_Home' AND Actions = 'View')
BEGIN
    INSERT INTO Permissions (Id, Name, Module, Actions, Description, CreatedAt, IsActived)
    VALUES (NEWID(), N'Xem Giám sát', 'Temperature_Home', 'View', N'Quyền xem giao diện dashboard giám sát nhiệt độ', @CreatedAt, 1)
END

-- 2. Temperature_Config.View
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Module = 'Temperature_Config' AND Actions = 'View')
BEGIN
    INSERT INTO Permissions (Id, Name, Module, Actions, Description, CreatedAt, IsActived)
    VALUES (NEWID(), N'Xem Cấu hình Nhiệt độ', 'Temperature_Config', 'View', N'Quyền xem danh sách cấu hình vị trí nhiệt độ', @CreatedAt, 1)
END

-- 3. Temperature_Config.Create
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Module = 'Temperature_Config' AND Actions = 'Create')
BEGIN
    INSERT INTO Permissions (Id, Name, Module, Actions, Description, CreatedAt, IsActived)
    VALUES (NEWID(), N'Thêm Cấu hình Nhiệt độ', 'Temperature_Config', 'Create', N'Quyền thêm mới cấu hình vị trí nhiệt độ', @CreatedAt, 1)
END

-- 4. Temperature_Config.Edit
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Module = 'Temperature_Config' AND Actions = 'Edit')
BEGIN
    INSERT INTO Permissions (Id, Name, Module, Actions, Description, CreatedAt, IsActived)
    VALUES (NEWID(), N'Sửa Cấu hình Nhiệt độ', 'Temperature_Config', 'Edit', N'Quyền cập nhật/sửa đổi cấu hình vị trí nhiệt độ', @CreatedAt, 1)
END

-- 5. Temperature_Config.Delete
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Module = 'Temperature_Config' AND Actions = 'Delete')
BEGIN
    INSERT INTO Permissions (Id, Name, Module, Actions, Description, CreatedAt, IsActived)
    VALUES (NEWID(), N'Xóa Cấu hình Nhiệt độ', 'Temperature_Config', 'Delete', N'Quyền xóa cấu hình vị trí nhiệt độ', @CreatedAt, 1)
END

-- 6. Temperature_Report.View
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Module = 'Temperature_Report' AND Actions = 'View')
BEGIN
    INSERT INTO Permissions (Id, Name, Module, Actions, Description, CreatedAt, IsActived)
    VALUES (NEWID(), N'Xem Báo cáo Nhiệt độ', 'Temperature_Report', 'View', N'Quyền xem lịch sử báo động nhiệt độ', @CreatedAt, 1)
END

-- 7. Temperature_Report.Export
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Module = 'Temperature_Report' AND Actions = 'Export')
BEGIN
    INSERT INTO Permissions (Id, Name, Module, Actions, Description, CreatedAt, IsActived)
    VALUES (NEWID(), N'Xuất Excel Báo cáo', 'Temperature_Report', 'Export', N'Quyền xuất dữ liệu báo cáo nhiệt độ ra file Excel', @CreatedAt, 1)
END

PRINT 'Da them thanh cong cac quyen (Permissions) cho module Temperature!'
GO
