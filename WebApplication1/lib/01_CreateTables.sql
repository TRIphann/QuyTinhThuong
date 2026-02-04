-- =====================================================
-- FILE 1: TẠO DATABASE VÀ CÁC BẢNG
-- HỆ THỐNG QUẢN LÝ QUỸ TÌNH THƯƠNG
-- =====================================================

USE master;
GO

-- Xóa database cũ nếu tồn tại
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'QLQuyTinhThuong')
BEGIN
    ALTER DATABASE QLQuyTinhThuong SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLQuyTinhThuong;
END
GO

-- Tạo database mới
CREATE DATABASE QLQuyTinhThuong;
GO

USE QLQuyTinhThuong;
GO

PRINT N'=====================================================';
PRINT N'BẮT ĐẦU TẠO CÁC BẢNG';
PRINT N'=====================================================';

-- =====================================================
-- BẢNG VAI TRÒ (ROLES)
-- =====================================================
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL
);
PRINT N'✓ Đã tạo bảng Roles';

-- =====================================================
-- BẢNG NGƯỜI DÙNG (USERS)
-- =====================================================
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(128) NOT NULL, -- SHA256 hash
    Email NVARCHAR(200) NULL,
    Phone NVARCHAR(20) NULL,
    Status NVARCHAR(50) DEFAULT 'Active'
);
PRINT N'✓ Đã tạo bảng Users';

-- =====================================================
-- BẢNG PHÂN QUYỀN NGƯỜI DÙNG (USER_ROLES)
-- =====================================================
CREATE TABLE User_Roles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE
);
PRINT N'✓ Đã tạo bảng User_Roles';

-- =====================================================
-- BẢNG NGƯỜI ĐÓNG GÓP (DONORS)
-- =====================================================
CREATE TABLE Donors (
    DonorId INT IDENTITY(1,1) PRIMARY KEY,
    DonorName NVARCHAR(200) NOT NULL,
    DonorType NVARCHAR(50) NOT NULL, -- Cá nhân, Doanh nghiệp, Tổ chức
    Address NVARCHAR(500) NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(200) NULL
);
PRINT N'✓ Đã tạo bảng Donors';

-- =====================================================
-- BẢNG QUYÊN GÓP (DONATIONS)
-- =====================================================
CREATE TABLE Donations (
    DonationId INT IDENTITY(1,1) PRIMARY KEY,
    DonorId INT NOT NULL,
    DonorUserId INT NULL, -- Liên kết với User nếu có tài khoản
    Amount DECIMAL(18,2) NOT NULL,
    DonationDate DATETIME DEFAULT GETDATE(),
    Method NVARCHAR(50) NOT NULL, -- Tiền mặt, Chuyển khoản, QR Code
    ReceivedBy INT NULL, -- Staff nhận tiền
    IsConfirmed BIT DEFAULT 1, -- Đã xác nhận
    FOREIGN KEY (DonorId) REFERENCES Donors(DonorId),
    FOREIGN KEY (DonorUserId) REFERENCES Users(UserId),
    FOREIGN KEY (ReceivedBy) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Donations';

-- =====================================================
-- BẢNG QUỸ TIỀN (FUNDS)
-- =====================================================
CREATE TABLE Funds (
    FundId INT IDENTITY(1,1) PRIMARY KEY,
    FundName NVARCHAR(200) NOT NULL,
    Balance DECIMAL(18,2) DEFAULT 0,
    LastUpdated DATETIME DEFAULT GETDATE()
);
PRINT N'✓ Đã tạo bảng Funds';

-- =====================================================
-- BẢNG ĐỐI TƯỢNG THỤ HƯỞNG (BENEFICIARIES)
-- =====================================================
CREATE TABLE Beneficiaries (
    BeneficiaryId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    BeneficiaryType NVARCHAR(100) NOT NULL, -- Loại đối tượng
    Address NVARCHAR(500) NULL,
    Description NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) DEFAULT N'Đã duyệt', -- Chờ duyệt, Đã duyệt, Từ chối
    CreatedBy INT NULL, -- Ai đã thêm
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Beneficiaries';

-- =====================================================
-- BẢNG HỒ SƠ ĐỀ NGHỊ HỖ TRỢ (SUPPORT_REQUESTS)
-- =====================================================
CREATE TABLE Support_Requests (
    RequestId INT IDENTITY(1,1) PRIMARY KEY,
    BeneficiaryId INT NOT NULL,
    RequestDate DATETIME DEFAULT GETDATE(),
    RequestedAmount DECIMAL(18,2) NULL,       -- Có thể NULL, quản lý quyết định sau
    SupportIssue NVARCHAR(MAX) NULL,          -- Vấn đề cần hỗ trợ
    Reason NVARCHAR(MAX) NULL,                -- Lý do hỗ trợ
    Status NVARCHAR(50) DEFAULT N'Chờ xét duyệt',
    CreatedBy INT NULL,                        -- Nhân viên tạo yêu cầu
    FOREIGN KEY (BeneficiaryId) REFERENCES Beneficiaries(BeneficiaryId),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Support_Requests';

-- =====================================================
-- BẢNG PHÊ DUYỆT HỒ SƠ (APPROVALS)
-- =====================================================
CREATE TABLE Approvals (
    ApprovalId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    ApprovedBy INT NOT NULL,
    ApprovalDate DATETIME DEFAULT GETDATE(),
    Result NVARCHAR(50) NOT NULL, -- Phê duyệt, Từ chối
    Note NVARCHAR(MAX) NULL,
    FOREIGN KEY (RequestId) REFERENCES Support_Requests(RequestId),
    FOREIGN KEY (ApprovedBy) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Approvals';

-- =====================================================
-- BẢNG KHOẢN CHI HỖ TRỢ (EXPENSES)
-- =====================================================
CREATE TABLE Expenses (
    ExpenseId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    ExpenseDate DATETIME DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(50) NOT NULL,
    PaidBy INT NOT NULL,
    FOREIGN KEY (RequestId) REFERENCES Support_Requests(RequestId),
    FOREIGN KEY (PaidBy) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Expenses';

-- =====================================================
-- BẢNG NHẬT KÝ HỆ THỐNG (LOGS)
-- =====================================================
CREATE TABLE Logs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(500) NOT NULL,
    TableName NVARCHAR(100) NULL,
    ActionTime DATETIME DEFAULT GETDATE(),
    OldData NVARCHAR(MAX) NULL,
    NewData NVARCHAR(MAX) NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Logs';

-- =====================================================
-- BẢNG CÔNG VIỆC HỖ TRỢ (SUPPORT_TASKS)
-- Manager tạo và phân công cho Staff
-- =====================================================
CREATE TABLE Support_Tasks (
    TaskId INT IDENTITY(1,1) PRIMARY KEY,
    RequestId INT NOT NULL,
    AssignedStaffId INT NULL,
    DonorUserId INT NULL,
    AssignedBy INT NULL,
    AssignedAt DATETIME NULL,
    ScheduledDate DATETIME NULL, -- Ngày dự kiến bắt đầu
    Amount DECIMAL(18,2) NOT NULL DEFAULT 0, -- Số tiền mục tiêu
    DonatedAmount DECIMAL(18,2) DEFAULT 0, -- Số tiền tình nguyện viên đã góp
    AdditionalAmount DECIMAL(18,2) DEFAULT 0,
    Status NVARCHAR(50) DEFAULT N'Chờ thực hiện',
    StartedAt DATETIME NULL,
    StaffNote NVARCHAR(MAX) NULL,
    StaffCompletedAt DATETIME NULL,
    ManagerNote NVARCHAR(MAX) NULL,
    ManagerVerifiedAt DATETIME NULL,
    SupportRequestType NVARCHAR(50) NULL, -- Tiền, Nhân lực
    SupportRequestReason NVARCHAR(MAX) NULL,
    SupportRequestAmount DECIMAL(18,2) NULL,
    SupportRequestPeopleCount INT NULL, -- Số người yêu cầu hỗ trợ (cho loại Nhân lực)
    SupportRequestAt DATETIME NULL,
    SupportResponseNote NVARCHAR(MAX) NULL,
    SupportResponseAt DATETIME NULL,
    SupportResponseStatus NVARCHAR(50) NULL, -- Đã duyệt, Từ chối
    SupportAssignedPeopleCount INT DEFAULT 0, -- Số người đã được giao (cho loại Nhân lực)
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RequestId) REFERENCES Support_Requests(RequestId),
    FOREIGN KEY (AssignedStaffId) REFERENCES Users(UserId),
    FOREIGN KEY (DonorUserId) REFERENCES Users(UserId),
    FOREIGN KEY (AssignedBy) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Support_Tasks';

-- =====================================================
-- BẢNG THÔNG BÁO (NOTIFICATIONS)
-- =====================================================
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50) NULL, -- Công việc mới, Hoàn thành, Phản ánh, ...
    RelatedTaskId INT NULL,
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (RelatedTaskId) REFERENCES Support_Tasks(TaskId)
);
PRINT N'✓ Đã tạo bảng Notifications';

-- =====================================================
-- BẢNG PHẢN ÁNH TỪ KHÁCH HÀNG (COMPLAINTS)
-- =====================================================
CREATE TABLE Complaints (
    ComplaintId INT IDENTITY(1,1) PRIMARY KEY,
    TaskId INT NOT NULL, -- Liên kết với task đã hoàn thành
    UserId INT NOT NULL, -- Khách hàng gửi phản ánh
    Content NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(50) DEFAULT N'Chờ xử lý', -- Chờ xử lý, Đã phản hồi, Đã đóng
    ResponseBy INT NULL, -- Manager phản hồi
    ResponseContent NVARCHAR(MAX) NULL, -- Phản hồi từ Manager
    ResponseAt DATETIME NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TaskId) REFERENCES Support_Tasks(TaskId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (ResponseBy) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Complaints';

-- =====================================================
-- BẢNG TÌNH NGUYỆN VIÊN THAM GIA HOẠT ĐỘNG (TASK_VOLUNTEERS)
-- =====================================================
CREATE TABLE Task_Volunteers (
    VolunteerId INT IDENTITY(1,1) PRIMARY KEY,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    RegisteredAt DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT N'Đăng ký', -- Đăng ký, Đã xác nhận, Đã tham gia, Hủy
    Note NVARCHAR(MAX) NULL,
    FOREIGN KEY (TaskId) REFERENCES Support_Tasks(TaskId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Task_Volunteers';

-- =====================================================
-- BẢNG QUYÊN GÓP CHO HOẠT ĐỘNG CỤ THỂ (TASK_DONATIONS)
-- =====================================================
CREATE TABLE Task_Donations (
    TaskDonationId INT IDENTITY(1,1) PRIMARY KEY,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    DonatedAt DATETIME DEFAULT GETDATE(),
    Note NVARCHAR(MAX) NULL,
    IsConfirmed BIT DEFAULT 1,
    FOREIGN KEY (TaskId) REFERENCES Support_Tasks(TaskId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Task_Donations';

-- =====================================================
-- BẢNG NGƯỜI HỖ TRỢ (SUPPORT_HELPERS)
-- Lưu danh sách staff được mời hỗ trợ công việc (loại Nhân lực)
-- =====================================================
CREATE TABLE Support_Helpers (
    HelperId INT IDENTITY(1,1) PRIMARY KEY,
    TaskId INT NOT NULL, -- Công việc cần hỗ trợ
    StaffId INT NOT NULL, -- Staff được mời hỗ trợ
    InvitedBy INT NULL, -- Manager gửi lời mời
    InvitedAt DATETIME DEFAULT GETDATE(), -- Thời gian mời
    Status NVARCHAR(50) DEFAULT N'Đang chờ', -- Đang chờ, Chấp nhận, Từ chối
    RespondedAt DATETIME NULL, -- Thời gian phản hồi
    StaffNote NVARCHAR(MAX) NULL, -- Ghi chú khi từ chối
    FOREIGN KEY (TaskId) REFERENCES Support_Tasks(TaskId),
    FOREIGN KEY (StaffId) REFERENCES Users(UserId),
    FOREIGN KEY (InvitedBy) REFERENCES Users(UserId)
);
PRINT N'✓ Đã tạo bảng Support_Helpers';

-- =====================================================
-- BẢNG YÊU CẦU PHÊ DUYỆT NGÂN SÁCH (BUDGET_APPROVALS)
-- Admin phê duyệt các yêu cầu chi tiền từ Manager
-- =====================================================
CREATE TABLE Budget_Approvals (
    ApprovalId INT IDENTITY(1,1) PRIMARY KEY,
    RequestType NVARCHAR(50) NOT NULL, -- 'CreateTask', 'AdditionalSupport'
    RequestedBy INT NOT NULL, -- Manager yêu cầu
    RequestedAt DATETIME DEFAULT GETDATE(),
    Amount DECIMAL(18,2) NOT NULL, -- Số tiền yêu cầu
    Description NVARCHAR(MAX) NULL, -- Mô tả yêu cầu
    RelatedTaskId INT NULL, -- Liên kết với task (nếu có)
    RelatedRequestId INT NULL, -- Liên kết với support request (nếu có)
    Status NVARCHAR(50) DEFAULT N'Chờ duyệt', -- Chờ duyệt, Đã duyệt, Từ chối
    ApprovedBy INT NULL, -- Admin phê duyệt
    ApprovedAt DATETIME NULL,
    RejectionReason NVARCHAR(MAX) NULL, -- Lý do từ chối
    -- Dữ liệu bổ sung cho CreateTask
    StaffIds NVARCHAR(MAX) NULL, -- JSON array của staff IDs
    ScheduledDate DATETIME NULL,
    ManagerNote NVARCHAR(MAX) NULL,
    FOREIGN KEY (RequestedBy) REFERENCES Users(UserId),
    FOREIGN KEY (ApprovedBy) REFERENCES Users(UserId),
    FOREIGN KEY (RelatedTaskId) REFERENCES Support_Tasks(TaskId),
    FOREIGN KEY (RelatedRequestId) REFERENCES Support_Requests(RequestId)
);
PRINT N'✓ Đã tạo bảng Budget_Approvals';

-- =====================================================
-- TẠO INDEX ĐỂ TỐI ƯU TRUY VẤN
-- =====================================================
CREATE INDEX IX_Donations_DonorUserId ON Donations(DonorUserId);
CREATE INDEX IX_Donations_IsConfirmed ON Donations(IsConfirmed);
CREATE INDEX IX_SupportTasks_Status ON Support_Tasks(Status);
CREATE INDEX IX_SupportTasks_AssignedStaffId ON Support_Tasks(AssignedStaffId);
CREATE INDEX IX_Notifications_UserId_IsRead ON Notifications(UserId, IsRead);
CREATE INDEX IX_Complaints_Status ON Complaints(Status);
CREATE INDEX IX_TaskVolunteers_TaskId ON Task_Volunteers(TaskId);
CREATE INDEX IX_TaskDonations_TaskId ON Task_Donations(TaskId);
CREATE INDEX IX_SupportHelpers_TaskId ON Support_Helpers(TaskId);
CREATE INDEX IX_SupportHelpers_StaffId ON Support_Helpers(StaffId);
CREATE INDEX IX_SupportHelpers_Status ON Support_Helpers(Status);
CREATE INDEX IX_BudgetApprovals_Status ON Budget_Approvals(Status);
CREATE INDEX IX_BudgetApprovals_RequestedBy ON Budget_Approvals(RequestedBy);

PRINT N'✓ Đã tạo các index';

PRINT N'=====================================================';
PRINT N'HOÀN TẤT TẠO CÁC BẢNG';
PRINT N'=====================================================';
GO
