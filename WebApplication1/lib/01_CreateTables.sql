-- =====================================================
-- FILE 1: TẠO CẤU TRÚC BẢNG
-- HỆ THỐNG QUẢN LÝ QUỸ TÌNH THƯƠNG
-- =====================================================

SET NOCOUNT ON;
GO

PRINT N'=====================================================';
PRINT N'BẮT ĐẦU TẠO DATABASE VÀ CẤU TRÚC BẢNG';
PRINT N'=====================================================';

-- Xóa database cũ nếu tồn tại
IF DB_ID('QLQuyTinhThuong') IS NOT NULL
BEGIN
    ALTER DATABASE QLQuyTinhThuong SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLQuyTinhThuong;
    PRINT N'✓ Đã xóa database cũ';
END

-- Tạo database mới với collation tiếng Việt
CREATE DATABASE QLQuyTinhThuong
COLLATE Vietnamese_100_CI_AS;
GO

USE QLQuyTinhThuong;
GO

PRINT N'✓ Đã tạo database QLQuyTinhThuong';
PRINT N'';
PRINT N'Đang tạo cấu trúc bảng...';

-- =====================================================
-- BẢNG NGƯỜI DÙNG VÀ PHÂN QUYỀN
-- =====================================================

-- Bảng Users (Người dùng hệ thống)
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Username NVARCHAR(100) UNIQUE NOT NULL,
    Password NVARCHAR(128) NOT NULL,
    Email NVARCHAR(150) NULL,
    Phone NVARCHAR(20) NULL,
    Status NVARCHAR(50) CHECK (Status IN (N'Hoạt động', N'Tạm khóa', N'Đã xóa')) DEFAULT N'Hoạt động'
);
PRINT N'✓ Đã tạo bảng Users';

-- Bảng Roles (Vai trò)
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(100) UNIQUE NOT NULL,
    Description NVARCHAR(500) NULL
);
PRINT N'✓ Đã tạo bảng Roles';

-- Bảng User_Roles (Phân vai trò cho người dùng)
CREATE TABLE User_Roles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE
);
PRINT N'✓ Đã tạo bảng User_Roles';

-- =====================================================
-- BẢNG QUẢN LÝ NGƯỜI ĐÓNG GÓP
-- =====================================================

-- Bảng Donors (Người đóng góp)
CREATE TABLE Donors (
    DonorId INT IDENTITY(1,1) PRIMARY KEY,
    DonorName NVARCHAR(200) NOT NULL,
    DonorType NVARCHAR(50) CHECK (DonorType IN (N'Cá nhân', N'Tổ chức', N'Doanh nghiệp')) NOT NULL,
    Address NVARCHAR(500) NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(150) NULL
);
PRINT N'✓ Đã tạo bảng Donors';

-- Bảng Donations (Khoản quyên góp)
CREATE TABLE Donations (
    DonationId INT IDENTITY(1,1) PRIMARY KEY,
    DonorId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
    DonationDate DATETIME DEFAULT GETDATE(),
    Method NVARCHAR(50) CHECK (Method IN (N'Tiền mặt', N'Chuyển khoản', N'QR Code', N'Thẻ')) NOT NULL,
    ReceivedBy INT NULL,
    FOREIGN KEY (DonorId) REFERENCES Donors(DonorId) ON DELETE NO ACTION,
    FOREIGN KEY (ReceivedBy) REFERENCES Users(UserId) ON DELETE SET NULL
);
PRINT N'✓ Đã tạo bảng Donations';

-- =====================================================
-- BẢNG QUẢN LÝ QUỸ
-- =====================================================

-- Bảng Funds (Quỹ tiền)
CREATE TABLE Funds (
    FundId INT IDENTITY(1,1) PRIMARY KEY,
    FundName NVARCHAR(200) NOT NULL,
    Balance DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Balance >= 0),
    LastUpdated DATETIME DEFAULT GETDATE()
);
PRINT N'✓ Đã tạo bảng Funds';

-- =====================================================
-- BẢNG QUẢN LÝ ĐỐI TƯỢNG THỤ HƯỞNG
-- =====================================================

-- Bảng Beneficiaries (Đối tượng được thụ hưởng)
CREATE TABLE Beneficiaries (
    BeneficiaryId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    BeneficiaryType NVARCHAR(100) CHECK (BeneficiaryType IN (
        N'Người nghèo', 
        N'Trẻ em khó khăn', 
        N'Người già neo đơn', 
        N'Người khuyết tật',
        N'Học sinh/Sinh viên nghèo',
        N'Bệnh nhân hiểm nghèo',
        N'Nạn nhân thiên tai',
        N'Khác'
    )) NOT NULL,
    Address NVARCHAR(500) NULL,
    Description NVARCHAR(MAX) NULL
);
PRINT N'✓ Đã tạo bảng Beneficiaries';

-- Bảng Support_Requests (Hồ sơ đề nghị hỗ trợ)
CREATE TABLE Support_Requests (
    RequestId INT IDENTITY(1,1) PRIMARY KEY,
    BeneficiaryId INT NOT NULL,
    RequestDate DATETIME DEFAULT GETDATE(),
    RequestedAmount DECIMAL(18,2) NOT NULL CHECK (RequestedAmount > 0),
    Reason NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) CHECK (Status IN (
        N'Chờ xét duyệt', 
        N'Đã phê duyệt', 
        N'Từ chối', 
        N'Đã chi trả',
        N'Đã hủy'
    )) DEFAULT N'Chờ xét duyệt',
    FOREIGN KEY (BeneficiaryId) REFERENCES Beneficiaries(BeneficiaryId) ON DELETE NO ACTION
);
PRINT N'✓ Đã tạo bảng Support_Requests';

-- Bảng Approvals (Phê duyệt hồ sơ)
CREATE TABLE Approvals (
    ApprovalId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    ApprovedBy INT NOT NULL,
    ApprovalDate DATETIME DEFAULT GETDATE(),
    Result NVARCHAR(50) CHECK (Result IN (N'Phê duyệt', N'Từ chối')) NOT NULL,
    Note NVARCHAR(MAX) NULL,
    FOREIGN KEY (RequestId) REFERENCES Support_Requests(RequestId) ON DELETE NO ACTION,
    FOREIGN KEY (ApprovedBy) REFERENCES Users(UserId) ON DELETE NO ACTION
);
PRINT N'✓ Đã tạo bảng Approvals';

-- Bảng Expenses (Khoản chi hỗ trợ)
CREATE TABLE Expenses (
    ExpenseId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
    ExpenseDate DATETIME DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(50) CHECK (PaymentMethod IN (N'Tiền mặt', N'Chuyển khoản', N'Thẻ')) NOT NULL,
    PaidBy INT NULL,
    FOREIGN KEY (RequestId) REFERENCES Support_Requests(RequestId) ON DELETE NO ACTION,
    FOREIGN KEY (PaidBy) REFERENCES Users(UserId) ON DELETE SET NULL
);
PRINT N'✓ Đã tạo bảng Expenses';

-- =====================================================
-- BẢNG NHẬT KÝ HỆ THỐNG
-- =====================================================

-- Bảng Logs (Nhật ký hệ thống)
CREATE TABLE Logs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(200) NOT NULL,
    TableName NVARCHAR(100) NULL,
    ActionTime DATETIME DEFAULT GETDATE(),
    OldData NVARCHAR(MAX) NULL,
    NewData NVARCHAR(MAX) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE SET NULL
);
PRINT N'✓ Đã tạo bảng Logs';

GO
PRINT N'✓ Đã tạo xong cấu trúc bảng';
PRINT N'';
