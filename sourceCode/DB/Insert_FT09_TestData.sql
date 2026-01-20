-- SQL Script để insert 45 dòng dữ liệu test vào bảng FT09 (REVO Datalog)
-- 9 REVO (RevoId từ 1-9), mỗi REVO có 5 dòng dữ liệu
-- Sử dụng để test trang báo cáo REVO (/revo/report)

-- Xóa dữ liệu cũ nếu cần (uncomment nếu muốn)
-- DELETE FROM FT09 WHERE RevoId BETWEEN 1 AND 9;

-- Insert 45 dòng dữ liệu test (9 REVO x 5 dòng)
INSERT INTO FT09 (Id, CreatedAt, CreatedMachine, RevoId, RevoName, Work, Part, Rev, ColorCode, Mandrel, MandrelStart, StepId, StepName, StartedAt, EndedAt)
VALUES
    -- ========== REVO 1 (5 dòng) ==========
    (NEWID(), GETDATE(), 'MACHINE-001', 1, 'REVO 1', 'WORK-001', 'PART-A', 'REV-1.0', 'RED', 'MANDREL-01', 'MANDREL-START-01', 1, 'Step 1 - Preheating', DATEADD(day, -5, GETDATE()), DATEADD(day, -5, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-001', 1, 'REVO 1', 'WORK-001', 'PART-A', 'REV-1.0', 'RED', 'MANDREL-01', 'MANDREL-START-01', 2, 'Step 2 - Heating', DATEADD(day, -4, GETDATE()), DATEADD(day, -4, DATEADD(hour, 3, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-001', 1, 'REVO 1', 'WORK-002', 'PART-B', 'REV-1.1', 'BLUE', 'MANDREL-02', 'MANDREL-START-02', 3, 'Step 3 - Cooling', DATEADD(hour, -2, GETDATE()), NULL),
    (NEWID(), GETDATE(), 'MACHINE-001', 1, 'REVO 1', 'WORK-002', 'PART-B', 'REV-1.1', 'BLUE', 'MANDREL-02', 'MANDREL-START-02', 4, 'Step 4 - Finalizing', DATEADD(day, -3, GETDATE()), DATEADD(day, -3, DATEADD(hour, 1, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-001', 1, 'REVO 1', 'WORK-003', 'PART-C', 'REV-1.2', 'GREEN', 'MANDREL-03', 'MANDREL-START-03', 5, 'Step 5 - Quality Check', DATEADD(day, -2, GETDATE()), DATEADD(day, -2, DATEADD(hour, 1, GETDATE()))),
    
    -- ========== REVO 2 (5 dòng) ==========
    (NEWID(), GETDATE(), 'MACHINE-002', 2, 'REVO 2', 'WORK-004', 'PART-D', 'REV-2.0', 'YELLOW', 'MANDREL-04', 'MANDREL-START-04', 1, 'Step 1 - Preheating', DATEADD(day, -1, GETDATE()), DATEADD(day, -1, DATEADD(hour, 1, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-002', 2, 'REVO 2', 'WORK-004', 'PART-D', 'REV-2.0', 'YELLOW', 'MANDREL-04', 'MANDREL-START-04', 2, 'Step 2 - Heating', DATEADD(hour, -5, GETDATE()), DATEADD(hour, -2, GETDATE())),
    (NEWID(), GETDATE(), 'MACHINE-002', 2, 'REVO 2', 'WORK-005', 'PART-E', 'REV-2.1', 'ORANGE', 'MANDREL-05', 'MANDREL-START-05', 3, 'Step 3 - Cooling', DATEADD(day, -6, GETDATE()), DATEADD(day, -6, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-002', 2, 'REVO 2', 'WORK-005', 'PART-E', 'REV-2.1', 'ORANGE', 'MANDREL-05', 'MANDREL-START-05', 4, 'Step 4 - Finalizing', DATEADD(day, -7, GETDATE()), DATEADD(day, -7, DATEADD(hour, 1, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-002', 2, 'REVO 2', 'WORK-006', 'PART-F', 'REV-2.2', 'PURPLE', 'MANDREL-06', 'MANDREL-START-06', 5, 'Step 5 - Quality Check', DATEADD(day, -8, GETDATE()), DATEADD(day, -8, DATEADD(hour, 1, GETDATE()))),
    
    -- ========== REVO 3 (5 dòng) ==========
    (NEWID(), GETDATE(), 'MACHINE-003', 3, 'REVO 3', 'WORK-007', 'PART-G', 'REV-3.0', 'PINK', 'MANDREL-07', 'MANDREL-START-07', 1, 'Step 1 - Preheating', DATEADD(day, -9, GETDATE()), DATEADD(day, -9, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-003', 3, 'REVO 3', 'WORK-007', 'PART-G', 'REV-3.0', 'PINK', 'MANDREL-07', 'MANDREL-START-07', 2, 'Step 2 - Heating', DATEADD(day, -10, GETDATE()), DATEADD(day, -10, DATEADD(hour, 3, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-003', 3, 'REVO 3', 'WORK-008', 'PART-H', 'REV-3.1', 'CYAN', 'MANDREL-08', 'MANDREL-START-08', 3, 'Step 3 - Cooling', DATEADD(day, -11, GETDATE()), DATEADD(day, -11, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-003', 3, 'REVO 3', 'WORK-008', 'PART-H', 'REV-3.1', 'CYAN', 'MANDREL-08', 'MANDREL-START-08', 4, 'Step 4 - Finalizing', DATEADD(day, -12, GETDATE()), DATEADD(day, -12, DATEADD(hour, 1, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-003', 3, 'REVO 3', 'WORK-009', 'PART-I', 'REV-3.2', 'BROWN', 'MANDREL-09', 'MANDREL-START-09', 5, 'Step 5 - Quality Check', DATEADD(day, -13, GETDATE()), DATEADD(day, -13, DATEADD(hour, 1, GETDATE()))),
    
    -- ========== REVO 4 (5 dòng) ==========
    (NEWID(), GETDATE(), 'MACHINE-004', 4, 'REVO 4', 'WORK-010', 'PART-J', 'REV-4.0', 'SILVER', 'MANDREL-10', 'MANDREL-START-10', 1, 'Step 1 - Preheating', DATEADD(day, -14, GETDATE()), DATEADD(day, -14, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-004', 4, 'REVO 4', 'WORK-010', 'PART-J', 'REV-4.0', 'SILVER', 'MANDREL-10', 'MANDREL-START-10', 2, 'Step 2 - Heating', DATEADD(day, -15, GETDATE()), DATEADD(day, -15, DATEADD(hour, 3, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-004', 4, 'REVO 4', 'WORK-011', 'PART-K', 'REV-4.1', 'GOLD', 'MANDREL-11', 'MANDREL-START-11', 3, 'Step 3 - Cooling', DATEADD(day, -16, GETDATE()), DATEADD(day, -16, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-004', 4, 'REVO 4', 'WORK-011', 'PART-K', 'REV-4.1', 'GOLD', 'MANDREL-11', 'MANDREL-START-11', 4, 'Step 4 - Finalizing', DATEADD(day, -17, GETDATE()), DATEADD(day, -17, DATEADD(hour, 1, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-004', 4, 'REVO 4', 'WORK-012', 'PART-L', 'REV-4.2', 'BLACK', 'MANDREL-12', 'MANDREL-START-12', 5, 'Step 5 - Quality Check', DATEADD(day, -18, GETDATE()), DATEADD(day, -18, DATEADD(hour, 1, GETDATE()))),
    
    -- ========== REVO 5 (5 dòng) ==========
    (NEWID(), GETDATE(), 'MACHINE-005', 5, 'REVO 5', 'WORK-013', 'PART-M', 'REV-5.0', 'WHITE', 'MANDREL-13', 'MANDREL-START-13', 1, 'Step 1 - Preheating', DATEADD(day, -19, GETDATE()), DATEADD(day, -19, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-005', 5, 'REVO 5', 'WORK-013', 'PART-M', 'REV-5.0', 'WHITE', 'MANDREL-13', 'MANDREL-START-13', 2, 'Step 2 - Heating', DATEADD(day, -20, GETDATE()), DATEADD(day, -20, DATEADD(hour, 3, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-005', 5, 'REVO 5', 'WORK-014', 'PART-N', 'REV-5.1', 'GRAY', 'MANDREL-14', 'MANDREL-START-14', 3, 'Step 3 - Cooling', DATEADD(day, -21, GETDATE()), DATEADD(day, -21, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-005', 5, 'REVO 5', 'WORK-014', 'PART-N', 'REV-5.1', 'GRAY', 'MANDREL-14', 'MANDREL-START-14', 4, 'Step 4 - Finalizing', DATEADD(day, -22, GETDATE()), DATEADD(day, -22, DATEADD(hour, 1, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-005', 5, 'REVO 5', 'WORK-015', 'PART-O', 'REV-5.2', 'MAROON', 'MANDREL-15', 'MANDREL-START-15', 5, 'Step 5 - Quality Check', DATEADD(day, -23, GETDATE()), DATEADD(day, -23, DATEADD(hour, 1, GETDATE()))),
    
    -- ========== REVO 6 (5 dòng) ==========
    (NEWID(), GETDATE(), 'MACHINE-006', 6, 'REVO 6', 'WORK-016', 'PART-P', 'REV-6.0', 'NAVY', 'MANDREL-16', 'MANDREL-START-16', 1, 'Step 1 - Preheating', DATEADD(day, -24, GETDATE()), DATEADD(day, -24, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-006', 6, 'REVO 6', 'WORK-016', 'PART-P', 'REV-6.0', 'NAVY', 'MANDREL-16', 'MANDREL-START-16', 2, 'Step 2 - Heating', DATEADD(day, -25, GETDATE()), DATEADD(day, -25, DATEADD(hour, 3, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-006', 6, 'REVO 6', 'WORK-017', 'PART-Q', 'REV-6.1', 'TEAL', 'MANDREL-17', 'MANDREL-START-17', 3, 'Step 3 - Cooling', DATEADD(day, -26, GETDATE()), DATEADD(day, -26, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-006', 6, 'REVO 6', 'WORK-017', 'PART-Q', 'REV-6.1', 'TEAL', 'MANDREL-17', 'MANDREL-START-17', 4, 'Step 4 - Finalizing', DATEADD(day, -27, GETDATE()), DATEADD(day, -27, DATEADD(hour, 1, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-006', 6, 'REVO 6', 'WORK-018', 'PART-R', 'REV-6.2', 'OLIVE', 'MANDREL-18', 'MANDREL-START-18', 5, 'Step 5 - Quality Check', DATEADD(day, -28, GETDATE()), DATEADD(day, -28, DATEADD(hour, 1, GETDATE()))),
    
    -- ========== REVO 7 (5 dòng) ==========
    (NEWID(), GETDATE(), 'MACHINE-007', 7, 'REVO 7', 'WORK-019', 'PART-S', 'REV-7.0', 'LIME', 'MANDREL-19', 'MANDREL-START-19', 1, 'Step 1 - Preheating', DATEADD(day, -29, GETDATE()), DATEADD(day, -29, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-007', 7, 'REVO 7', 'WORK-019', 'PART-S', 'REV-7.0', 'LIME', 'MANDREL-19', 'MANDREL-START-19', 2, 'Step 2 - Heating', DATEADD(day, -30, GETDATE()), DATEADD(day, -30, DATEADD(hour, 3, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-007', 7, 'REVO 7', 'WORK-020', 'PART-T', 'REV-7.1', 'AQUA', 'MANDREL-20', 'MANDREL-START-20', 3, 'Step 3 - Cooling', DATEADD(day, -31, GETDATE()), DATEADD(day, -31, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-007', 7, 'REVO 7', 'WORK-020', 'PART-T', 'REV-7.1', 'AQUA', 'MANDREL-20', 'MANDREL-START-20', 4, 'Step 4 - Finalizing', DATEADD(day, -32, GETDATE()), DATEADD(day, -32, DATEADD(hour, 1, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-007', 7, 'REVO 7', 'WORK-021', 'PART-U', 'REV-7.2', 'FUCHSIA', 'MANDREL-21', 'MANDREL-START-21', 5, 'Step 5 - Quality Check', DATEADD(day, -33, GETDATE()), DATEADD(day, -33, DATEADD(hour, 1, GETDATE()))),
    
    -- ========== REVO 8 (5 dòng) ==========
    (NEWID(), GETDATE(), 'MACHINE-008', 8, 'REVO 8', 'WORK-022', 'PART-V', 'REV-8.0', 'CORAL', 'MANDREL-22', 'MANDREL-START-22', 1, 'Step 1 - Preheating', DATEADD(day, -34, GETDATE()), DATEADD(day, -34, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-008', 8, 'REVO 8', 'WORK-022', 'PART-V', 'REV-8.0', 'CORAL', 'MANDREL-22', 'MANDREL-START-22', 2, 'Step 2 - Heating', DATEADD(day, -35, GETDATE()), DATEADD(day, -35, DATEADD(hour, 3, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-008', 8, 'REVO 8', 'WORK-023', 'PART-W', 'REV-8.1', 'SALMON', 'MANDREL-23', 'MANDREL-START-23', 3, 'Step 3 - Cooling', DATEADD(day, -36, GETDATE()), DATEADD(day, -36, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-008', 8, 'REVO 8', 'WORK-023', 'PART-W', 'REV-8.1', 'SALMON', 'MANDREL-23', 'MANDREL-START-23', 4, 'Step 4 - Finalizing', DATEADD(day, -37, GETDATE()), DATEADD(day, -37, DATEADD(hour, 1, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-008', 8, 'REVO 8', 'WORK-024', 'PART-X', 'REV-8.2', 'KHAKI', 'MANDREL-24', 'MANDREL-START-24', 5, 'Step 5 - Quality Check', DATEADD(day, -38, GETDATE()), DATEADD(day, -38, DATEADD(hour, 1, GETDATE()))),
    
    -- ========== REVO 9 (5 dòng) ==========
    (NEWID(), GETDATE(), 'MACHINE-009', 9, 'REVO 9', 'WORK-025', 'PART-Y', 'REV-9.0', 'IVORY', 'MANDREL-25', 'MANDREL-START-25', 1, 'Step 1 - Preheating', DATEADD(day, -39, GETDATE()), DATEADD(day, -39, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-009', 9, 'REVO 9', 'WORK-025', 'PART-Y', 'REV-9.0', 'IVORY', 'MANDREL-25', 'MANDREL-START-25', 2, 'Step 2 - Heating', DATEADD(day, -40, GETDATE()), DATEADD(day, -40, DATEADD(hour, 3, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-009', 9, 'REVO 9', 'WORK-026', 'PART-Z', 'REV-9.1', 'LAVENDER', 'MANDREL-26', 'MANDREL-START-26', 3, 'Step 3 - Cooling', DATEADD(day, -41, GETDATE()), DATEADD(day, -41, DATEADD(hour, 2, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-009', 9, 'REVO 9', 'WORK-026', 'PART-Z', 'REV-9.1', 'LAVENDER', 'MANDREL-26', 'MANDREL-START-26', 4, 'Step 4 - Finalizing', DATEADD(day, -42, GETDATE()), DATEADD(day, -42, DATEADD(hour, 1, GETDATE()))),
    (NEWID(), GETDATE(), 'MACHINE-009', 9, 'REVO 9', 'WORK-027', 'PART-AA', 'REV-9.2', 'MINT', 'MANDREL-27', 'MANDREL-START-27', 5, 'Step 5 - Quality Check', DATEADD(day, -43, GETDATE()), DATEADD(day, -43, DATEADD(hour, 1, GETDATE())));

-- Kiểm tra dữ liệu đã insert
SELECT 
    RevoId,
    RevoName,
    COUNT(*) AS TotalRecords,
    MIN(StartedAt) AS EarliestStart,
    MAX(StartedAt) AS LatestStart
FROM FT09
WHERE RevoId BETWEEN 1 AND 9
GROUP BY RevoId, RevoName
ORDER BY RevoId;

-- Xem chi tiết tất cả dữ liệu
SELECT 
    Id,
    RevoId,
    RevoName,
    StepId,
    StepName,
    Part,
    Rev,
    ColorCode,
    StartedAt,
    EndedAt,
    DATEDIFF(MINUTE, StartedAt, EndedAt) AS DurationMinutes,
    CreatedAt
FROM FT09
WHERE RevoId BETWEEN 1 AND 9
ORDER BY RevoId, StartedAt DESC;
