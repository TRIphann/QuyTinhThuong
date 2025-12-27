-- =====================================================
-- FILE 3: TẠO STORED PROCEDURES VÀ CẤU HÌNH BỔ SUNG
-- HỆ THỐNG QUẢN LÝ QUỸ TÌNH THƯƠNG
-- =====================================================

USE QLQuyTinhThuong;
GO

SET NOCOUNT ON;
GO

PRINT N'=====================================================';
PRINT N'BẮT ĐẦU TẠO STORED PROCEDURES';
PRINT N'=====================================================';

-- =====================================================
-- STORED PROCEDURE: ĐĂNG NHẬP
-- =====================================================
IF OBJECT_ID('sp_Login', 'P') IS NOT NULL
    DROP PROCEDURE sp_Login;
GO

CREATE PROCEDURE sp_Login
    @Username NVARCHAR(100),
    @Password NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.FullName,
        u.Username,
        u.Email,
        u.Phone,
        u.Status,
        r.RoleId,
        r.RoleName,
        r.Description AS RoleDescription
    FROM Users u
    INNER JOIN User_Roles ur ON u.UserId = ur.UserId
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    WHERE u.Username = @Username 
        AND u.Password = @Password
        AND u.Status = N'Hoạt động';
END;
GO

PRINT N'✓ Đã tạo stored procedure sp_Login';

-- =====================================================
-- STORED PROCEDURE: CẬP NHẬT SỐ DƯ QUỸ
-- =====================================================
IF OBJECT_ID('sp_UpdateFundBalance', 'P') IS NOT NULL
    DROP PROCEDURE sp_UpdateFundBalance;
GO

CREATE PROCEDURE sp_UpdateFundBalance
    @FundId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalDonations DECIMAL(18,2);
    DECLARE @TotalExpenses DECIMAL(18,2);
    DECLARE @NewBalance DECIMAL(18,2);
    
    -- Tính tổng quyên góp
    SELECT @TotalDonations = ISNULL(SUM(Amount), 0)
    FROM Donations;
    
    -- Tính tổng chi
    SELECT @TotalExpenses = ISNULL(SUM(Amount), 0)
    FROM Expenses;
    
    -- Tính số dư mới
    SET @NewBalance = @TotalDonations - @TotalExpenses;
    
    -- Cập nhật số dư
    UPDATE Funds
    SET Balance = @NewBalance,
        LastUpdated = GETDATE()
    WHERE FundId = @FundId;
    
    SELECT @NewBalance AS NewBalance;
END;
GO

PRINT N'✓ Đã tạo stored procedure sp_UpdateFundBalance';

-- =====================================================
-- STORED PROCEDURE: LẤY THÔNG TIN NGƯỜI DÙNG THEO ID
-- =====================================================
IF OBJECT_ID('sp_GetUserById', 'P') IS NOT NULL
    DROP PROCEDURE sp_GetUserById;
GO

CREATE PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.FullName,
        u.Username,
        u.Email,
        u.Phone,
        u.Status,
        r.RoleId,
        r.RoleName,
        r.Description AS RoleDescription
    FROM Users u
    INNER JOIN User_Roles ur ON u.UserId = ur.UserId
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    WHERE u.UserId = @UserId;
END;
GO

PRINT N'✓ Đã tạo stored procedure sp_GetUserById';

-- =====================================================
-- TRIGGER: GHI NHẬT KÝ KHI THÊM QUYÊN GÓP
-- =====================================================
IF OBJECT_ID('trg_Donations_Insert', 'TR') IS NOT NULL
    DROP TRIGGER trg_Donations_Insert;
GO

CREATE TRIGGER trg_Donations_Insert
ON Donations
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Logs (UserId, Action, TableName, ActionTime, NewData)
    SELECT 
        i.ReceivedBy,
        N'Thêm khoản quyên góp',
        N'Donations',
        GETDATE(),
        CONCAT(N'DonationId: ', i.DonationId, N', Số tiền: ', FORMAT(i.Amount, 'N0'), N' VNĐ')
    FROM inserted i;
END;
GO

PRINT N'✓ Đã tạo trigger trg_Donations_Insert';

-- =====================================================
-- TRIGGER: GHI NHẬT KÝ KHI PHÊ DUYỆT HỒ SƠ
-- =====================================================
IF OBJECT_ID('trg_Approvals_Insert', 'TR') IS NOT NULL
    DROP TRIGGER trg_Approvals_Insert;
GO

CREATE TRIGGER trg_Approvals_Insert
ON Approvals
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Logs (UserId, Action, TableName, ActionTime, NewData)
    SELECT 
        i.ApprovedBy,
        N'Phê duyệt hồ sơ',
        N'Approvals',
        GETDATE(),
        CONCAT(N'RequestId: ', i.RequestId, N', Kết quả: ', i.Result)
    FROM inserted i;
    
    -- Cập nhật trạng thái hồ sơ
    UPDATE Support_Requests
    SET Status = CASE 
        WHEN i.Result = N'Phê duyệt' THEN N'Đã phê duyệt'
        WHEN i.Result = N'Từ chối' THEN N'Từ chối'
    END
    FROM Support_Requests sr
    INNER JOIN inserted i ON sr.RequestId = i.RequestId;
END;
GO

PRINT N'✓ Đã tạo trigger trg_Approvals_Insert';

-- =====================================================
-- TRIGGER: CẬP NHẬT SỐ DƯ QUỸ KHI CÓ CHI
-- =====================================================
IF OBJECT_ID('trg_Expenses_Insert', 'TR') IS NOT NULL
    DROP TRIGGER trg_Expenses_Insert;
GO

CREATE TRIGGER trg_Expenses_Insert
ON Expenses
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Ghi nhật ký
    INSERT INTO Logs (UserId, Action, TableName, ActionTime, NewData)
    SELECT 
        i.PaidBy,
        N'Thêm khoản chi',
        N'Expenses',
        GETDATE(),
        CONCAT(N'ExpenseId: ', i.ExpenseId, N', Số tiền: ', FORMAT(i.Amount, 'N0'), N' VNĐ')
    FROM inserted i;
    
    -- Cập nhật trạng thái hồ sơ
    UPDATE Support_Requests
    SET Status = N'Đã chi trả'
    FROM Support_Requests sr
    INNER JOIN inserted i ON sr.RequestId = i.RequestId;
    
    -- Cập nhật số dư quỹ (giảm số tiền chi)
    UPDATE Funds
    SET Balance = Balance - (SELECT SUM(Amount) FROM inserted),
        LastUpdated = GETDATE()
    WHERE FundId = 1;
END;
GO

PRINT N'✓ Đã tạo trigger trg_Expenses_Insert';

PRINT N'';
PRINT N'=====================================================';
PRINT N'✓ HOÀN TẤT TẠO STORED PROCEDURES VÀ TRIGGERS';
PRINT N'=====================================================';
PRINT N'';
GO
