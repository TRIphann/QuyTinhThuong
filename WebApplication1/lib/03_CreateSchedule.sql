-- =====================================================
-- FILE 3: TẠO STORED PROCEDURES VÀ TRIGGERS
-- HỆ THỐNG QUẢN LÝ QUỸ TÌNH THƯƠNG
-- =====================================================

USE QLQuyTinhThuong;
GO

PRINT N'=====================================================';
PRINT N'BẮT ĐẦU TẠO STORED PROCEDURES VÀ TRIGGERS';
PRINT N'=====================================================';

-- =====================================================
-- TRIGGER: Tự động cập nhật số dư quỹ khi có donation mới
-- =====================================================
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Donations_UpdateFund')
    DROP TRIGGER TR_Donations_UpdateFund;
GO

CREATE TRIGGER TR_Donations_UpdateFund
ON Donations
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @amount DECIMAL(18,2);
    SELECT @amount = SUM(Amount) FROM inserted WHERE IsConfirmed = 1;
    
    IF @amount IS NOT NULL AND @amount > 0
    BEGIN
        UPDATE Funds 
        SET Balance = Balance + @amount,
            LastUpdated = GETDATE()
        WHERE FundId = 1;
    END
END
GO

PRINT N'✓ Đã tạo trigger TR_Donations_UpdateFund';

-- =====================================================
-- TRIGGER: Tự động trừ tiền quỹ khi Staff bắt đầu thực hiện
-- =====================================================
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_SupportTasks_DeductFund')
    DROP TRIGGER TR_SupportTasks_DeductFund;
GO

CREATE TRIGGER TR_SupportTasks_DeductFund
ON Support_Tasks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Chỉ trừ tiền khi chuyển từ "Chờ thực hiện" sang "Đang thực hiện"
    DECLARE @taskId INT, @amount DECIMAL(18,2);
    
    SELECT @taskId = i.TaskId, @amount = i.Amount
    FROM inserted i
    INNER JOIN deleted d ON i.TaskId = d.TaskId
    WHERE d.Status = N'Chờ thực hiện' 
      AND i.Status = N'Đang thực hiện'
      AND i.StartedAt IS NOT NULL;
    
    IF @amount IS NOT NULL AND @amount > 0
    BEGIN
        UPDATE Funds 
        SET Balance = Balance - @amount,
            LastUpdated = GETDATE()
        WHERE FundId = 1;
    END
END
GO

PRINT N'✓ Đã tạo trigger TR_SupportTasks_DeductFund';

-- =====================================================
-- TRIGGER: Tự động trừ tiền bổ sung khi Manager phê duyệt
-- =====================================================
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_SupportTasks_DeductAdditional')
    DROP TRIGGER TR_SupportTasks_DeductAdditional;
GO

CREATE TRIGGER TR_SupportTasks_DeductAdditional
ON Support_Tasks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Trừ tiền bổ sung khi Manager phê duyệt yêu cầu hỗ trợ
    DECLARE @additionalAmount DECIMAL(18,2);
    
    SELECT @additionalAmount = i.AdditionalAmount - ISNULL(d.AdditionalAmount, 0)
    FROM inserted i
    INNER JOIN deleted d ON i.TaskId = d.TaskId
    WHERE i.AdditionalAmount > ISNULL(d.AdditionalAmount, 0);
    
    IF @additionalAmount IS NOT NULL AND @additionalAmount > 0
    BEGIN
        UPDATE Funds 
        SET Balance = Balance - @additionalAmount,
            LastUpdated = GETDATE()
        WHERE FundId = 1;
    END
END
GO

PRINT N'✓ Đã tạo trigger TR_SupportTasks_DeductAdditional';

-- =====================================================
-- STORED PROCEDURE: Lấy thống kê tổng quan
-- =====================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetDashboardStats')
    DROP PROCEDURE SP_GetDashboardStats;
GO

CREATE PROCEDURE SP_GetDashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        (SELECT Balance FROM Funds WHERE FundId = 1) AS FundBalance,
        (SELECT COUNT(*) FROM Donors) AS TotalDonors,
        (SELECT COUNT(*) FROM Beneficiaries) AS TotalBeneficiaries,
        (SELECT SUM(Amount) FROM Donations WHERE IsConfirmed = 1) AS TotalDonations,
        (SELECT COUNT(*) FROM Support_Tasks WHERE Status = N'Hoàn thành') AS CompletedTasks,
        (SELECT COUNT(*) FROM Support_Tasks WHERE Status = N'Đang thực hiện') AS InProgressTasks,
        (SELECT COUNT(*) FROM Support_Tasks WHERE Status = N'Chờ thực hiện') AS PendingTasks,
        (SELECT COUNT(*) FROM Support_Tasks WHERE Status = N'Yêu cầu hỗ trợ') AS SupportRequestTasks,
        (SELECT COUNT(*) FROM Complaints WHERE Status = N'Chờ xử lý') AS PendingComplaints;
END
GO

PRINT N'✓ Đã tạo stored procedure SP_GetDashboardStats';

-- =====================================================
-- STORED PROCEDURE: Lấy lịch sử quyên góp của user
-- =====================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetUserDonations')
    DROP PROCEDURE SP_GetUserDonations;
GO

CREATE PROCEDURE SP_GetUserDonations
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        d.DonationId,
        d.Amount,
        d.DonationDate,
        d.Method,
        d.IsConfirmed,
        dn.DonorName
    FROM Donations d
    INNER JOIN Donors dn ON d.DonorId = dn.DonorId
    WHERE d.DonorUserId = @UserId
    ORDER BY d.DonationDate DESC;
END
GO

PRINT N'✓ Đã tạo stored procedure SP_GetUserDonations';

-- =====================================================
-- STORED PROCEDURE: Lấy công việc của Staff
-- =====================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetStaffTasks')
    DROP PROCEDURE SP_GetStaffTasks;
GO

CREATE PROCEDURE SP_GetStaffTasks
    @StaffId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.TaskId,
        t.Amount,
        t.AdditionalAmount,
        t.Status,
        t.StaffNote,
        t.StartedAt,
        t.StaffCompletedAt,
        t.CreatedAt,
        b.FullName AS BeneficiaryName,
        b.BeneficiaryType,
        b.Address AS BeneficiaryAddress,
        sr.Reason
    FROM Support_Tasks t
    INNER JOIN Support_Requests sr ON t.RequestId = sr.RequestId
    INNER JOIN Beneficiaries b ON sr.BeneficiaryId = b.BeneficiaryId
    WHERE t.AssignedStaffId = @StaffId
    ORDER BY 
        CASE t.Status 
            WHEN N'Đang thực hiện' THEN 1
            WHEN N'Yêu cầu hỗ trợ' THEN 2
            WHEN N'Chờ thực hiện' THEN 3
            ELSE 4 
        END,
        t.CreatedAt DESC;
END
GO

PRINT N'✓ Đã tạo stored procedure SP_GetStaffTasks';

-- =====================================================
-- STORED PROCEDURE: Lấy hỗ trợ đã hoàn thành (cho khách hàng xem)
-- =====================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GetCompletedSupports')
    DROP PROCEDURE SP_GetCompletedSupports;
GO

CREATE PROCEDURE SP_GetCompletedSupports
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.TaskId,
        t.Amount + ISNULL(t.AdditionalAmount, 0) AS TotalAmount,
        t.StaffNote,
        t.StaffCompletedAt,
        b.FullName AS BeneficiaryName,
        b.BeneficiaryType,
        b.Address AS BeneficiaryAddress,
        sr.Reason,
        u.FullName AS StaffName
    FROM Support_Tasks t
    INNER JOIN Support_Requests sr ON t.RequestId = sr.RequestId
    INNER JOIN Beneficiaries b ON sr.BeneficiaryId = b.BeneficiaryId
    INNER JOIN Users u ON t.AssignedStaffId = u.UserId
    WHERE t.Status = N'Hoàn thành'
    ORDER BY t.StaffCompletedAt DESC;
END
GO

PRINT N'✓ Đã tạo stored procedure SP_GetCompletedSupports';

PRINT N'=====================================================';
PRINT N'HOÀN TẤT TẠO STORED PROCEDURES VÀ TRIGGERS';
PRINT N'=====================================================';
GO

-- =====================================================
-- HƯỚNG DẪN CHẠY FILE
-- =====================================================
-- Chạy theo thứ tự:
-- 1. 01_CreateTables.sql - Tạo database và các bảng
-- 2. 02_InsertData.sql - Thêm dữ liệu mẫu
-- 3. 03_CreateSchedule.sql - Tạo stored procedures và triggers
-- =====================================================
