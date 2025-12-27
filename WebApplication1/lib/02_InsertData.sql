-- =====================================================
-- FILE 2: INSERT DỮ LIỆU MẪU
-- HỆ THỐNG QUẢN LÝ QUỸ TÌNH THƯƠNG
-- =====================================================

USE QLQuyTinhThuong;
GO

SET NOCOUNT ON;
GO

PRINT N'=====================================================';
PRINT N'BẮT ĐẦU THÊM DỮ LIỆU MẪU';
PRINT N'=====================================================';

-- =====================================================
-- INSERT VAI TRÒ (ROLES)
-- =====================================================
PRINT N'Đang thêm vai trò...';

SET IDENTITY_INSERT Roles ON;
INSERT INTO Roles (RoleId, RoleName, Description) VALUES
(1, N'ADMIN', N'Quản trị hệ thống - Toàn quyền quản lý'),
(2, N'STAFF', N'Nhân viên quỹ - Tiếp nhận đóng góp và lập hồ sơ'),
(3, N'ACCOUNTANT', N'Kế toán - Quản lý thu chi và báo cáo tài chính'),
(4, N'MANAGER', N'Ban quản lý - Phê duyệt hồ sơ và giám sát');
SET IDENTITY_INSERT Roles OFF;

PRINT N'✓ Đã thêm 4 vai trò';

-- =====================================================
-- INSERT USERS (Password: 123456789 - SHA256)
-- =====================================================
PRINT N'Đang thêm người dùng...';

SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserId, FullName, Username, Password, Email, Phone, Status) VALUES
(1, N'Nguyễn Văn Admin', 'admin', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'admin@quytt.vn', '0901234567', N'Hoạt động'),
(2, N'Trần Thị Lan', 'staff1', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'lan.tran@quytt.vn', '0901234568', N'Hoạt động'),
(3, N'Lê Văn Minh', 'staff2', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'minh.le@quytt.vn', '0901234569', N'Hoạt động'),
(4, N'Phạm Thị Hoa', 'accountant1', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'hoa.pham@quytt.vn', '0901234570', N'Hoạt động'),
(5, N'Võ Văn Dũng', 'manager1', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'dung.vo@quytt.vn', '0901234571', N'Hoạt động'),
(6, N'Hoàng Thị Mai', 'staff3', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'mai.hoang@quytt.vn', '0901234572', N'Hoạt động'),
(7, N'Đặng Văn Hùng', 'accountant2', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'hung.dang@quytt.vn', '0901234573', N'Hoạt động'),
(8, N'Bùi Thị Ngọc', 'manager2', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'ngoc.bui@quytt.vn', '0901234574', N'Hoạt động');
SET IDENTITY_INSERT Users OFF;

PRINT N'✓ Đã thêm 8 người dùng';

-- =====================================================
-- INSERT USER_ROLES (PHÂN QUYỀN)
-- =====================================================
PRINT N'Đang phân quyền cho người dùng...';

INSERT INTO User_Roles (UserId, RoleId) VALUES
(1, 1),  -- admin -> ADMIN
(2, 2),  -- staff1 -> STAFF
(3, 2),  -- staff2 -> STAFF
(4, 3),  -- accountant1 -> ACCOUNTANT
(5, 4),  -- manager1 -> MANAGER
(6, 2),  -- staff3 -> STAFF
(7, 3),  -- accountant2 -> ACCOUNTANT
(8, 4);  -- manager2 -> MANAGER

PRINT N'✓ Đã phân quyền cho 8 người dùng';

-- =====================================================
-- INSERT DONORS (NGƯỜI ĐÓNG GÓP)
-- =====================================================
PRINT N'Đang thêm người đóng góp...';

SET IDENTITY_INSERT Donors ON;
INSERT INTO Donors (DonorId, DonorName, DonorType, Address, Phone, Email) VALUES
(1, N'Nguyễn Văn A', N'Cá nhân', N'123 Lê Lợi, Q.1, TP.HCM', '0987654321', 'vana@gmail.com'),
(2, N'Công ty TNHH ABC', N'Doanh nghiệp', N'456 Nguyễn Huệ, Q.1, TP.HCM', '0281234567', 'contact@abc.com'),
(3, N'Trần Thị B', N'Cá nhân', N'789 Trần Hưng Đạo, Q.5, TP.HCM', '0987654322', 'tranb@gmail.com'),
(4, N'Tổ chức Từ thiện XYZ', N'Tổ chức', N'321 Điện Biên Phủ, Q.3, TP.HCM', '0281234568', 'info@xyz.org'),
(5, N'Lê Văn C', N'Cá nhân', N'654 Cách Mạng Tháng 8, Q.10, TP.HCM', '0987654323', 'levanc@gmail.com'),
(6, N'Công ty Cổ phần DEF', N'Doanh nghiệp', N'147 Hai Bà Trưng, Q.3, TP.HCM', '0281234569', 'contact@def.com.vn'),
(7, N'Phạm Thị D', N'Cá nhân', N'258 Lý Thường Kiệt, Q.10, TP.HCM', '0987654324', 'phamd@gmail.com'),
(8, N'Hoàng Văn E', N'Cá nhân', N'369 Võ Văn Tần, Q.3, TP.HCM', '0987654325', 'hoange@gmail.com'),
(9, N'Ngân hàng ABC', N'Doanh nghiệp', N'741 Nguyễn Trãi, Q.1, TP.HCM', '0281234570', 'csr@abcbank.vn'),
(10, N'Võ Thị F', N'Cá nhân', N'852 Phan Đình Phùng, Q.5, TP.HCM', '0987654326', 'vothif@gmail.com');
SET IDENTITY_INSERT Donors OFF;

PRINT N'✓ Đã thêm 10 người đóng góp';

-- =====================================================
-- INSERT DONATIONS (KHOẢN QUYÊN GÓP)
-- =====================================================
PRINT N'Đang thêm khoản quyên góp...';

SET IDENTITY_INSERT Donations ON;
INSERT INTO Donations (DonationId, DonorId, Amount, DonationDate, Method, ReceivedBy) VALUES
(1, 1, 5000000, '2025-01-15 09:30:00', N'Tiền mặt', 2),
(2, 2, 50000000, '2025-01-20 10:00:00', N'Chuyển khoản', 2),
(3, 3, 2000000, '2025-02-05 14:20:00', N'Tiền mặt', 3),
(4, 4, 100000000, '2025-02-10 11:15:00', N'Chuyển khoản', 2),
(5, 5, 3000000, '2025-02-25 16:45:00', N'QR Code', 3),
(6, 6, 75000000, '2025-03-01 09:00:00', N'Chuyển khoản', 2),
(7, 7, 1500000, '2025-03-10 13:30:00', N'Tiền mặt', 6),
(8, 8, 4000000, '2025-03-15 10:20:00', N'QR Code', 6),
(9, 9, 200000000, '2025-03-20 08:00:00', N'Chuyển khoản', 2),
(10, 10, 2500000, '2025-04-05 15:00:00', N'Tiền mặt', 3),
(11, 1, 10000000, '2025-04-15 11:00:00', N'Chuyển khoản', 2),
(12, 3, 3500000, '2025-05-01 14:00:00', N'QR Code', 6);
SET IDENTITY_INSERT Donations OFF;

PRINT N'✓ Đã thêm 12 khoản quyên góp';

-- =====================================================
-- INSERT FUNDS (QUỸ TIỀN)
-- =====================================================
PRINT N'Đang khởi tạo quỹ...';

SET IDENTITY_INSERT Funds ON;
INSERT INTO Funds (FundId, FundName, Balance, LastUpdated) VALUES
(1, N'Quỹ Tình Thương', 456500000, GETDATE());
SET IDENTITY_INSERT Funds OFF;

PRINT N'✓ Đã khởi tạo quỹ với số dư: 456,500,000 VNĐ';

-- =====================================================
-- INSERT BENEFICIARIES (ĐỐI TƯỢNG THỤ HƯỞNG)
-- =====================================================
PRINT N'Đang thêm đối tượng thụ hưởng...';

SET IDENTITY_INSERT Beneficiaries ON;
INSERT INTO Beneficiaries (BeneficiaryId, FullName, BeneficiaryType, Address, Description) VALUES
(1, N'Nguyễn Thị Mai', N'Bệnh nhân hiểm nghèo', N'123 Xã Tân Lập, Huyện Bình Chánh, TP.HCM', N'Bệnh nhân ung thư giai đoạn cuối, gia đình khó khăn'),
(2, N'Trần Văn Bình', N'Người khuyết tật', N'456 Xã Phước Kiển, Huyện Nhà Bè, TP.HCM', N'Khuyết tật bẩm sinh, không có khả năng lao động'),
(3, N'Lê Thị Hoa', N'Người già neo đơn', N'789 Xã Long Trường, Quận 9, TP.HCM', N'Cụ bà 78 tuổi sống một mình, không con cái'),
(4, N'Phạm Văn Tùng', N'Học sinh/Sinh viên nghèo', N'321 Xã Bình Hưng, Huyện Bình Chánh, TP.HCM', N'Sinh viên mồ côi, học giỏi nhưng hoàn cảnh khó khăn'),
(5, N'Hoàng Thị Lan', N'Trẻ em khó khăn', N'654 Xã Phú Xuân, Huyện Nhà Bè, TP.HCM', N'Trẻ mồ côi cha, mẹ bệnh nặng không có khả năng nuôi con'),
(6, N'Võ Văn Đức', N'Nạn nhân thiên tai', N'147 Xã Tân Nhựt, Huyện Bình Chánh, TP.HCM', N'Gia đình bị thiệt hại nặng do lũ lụt'),
(7, N'Đặng Thị Ngọc', N'Người nghèo', N'258 Xã Phước Lộc, Huyện Nhà Bè, TP.HCM', N'Gia đình nghèo 5 người, thu nhập thấp'),
(8, N'Bùi Văn Hải', N'Bệnh nhân hiểm nghèo', N'369 Xã Long Phước, Quận 9, TP.HCM', N'Bệnh thận mãn tính, cần lọc máu thường xuyên');
SET IDENTITY_INSERT Beneficiaries OFF;

PRINT N'✓ Đã thêm 8 đối tượng thụ hưởng';

-- =====================================================
-- INSERT SUPPORT_REQUESTS (HỒ SƠ ĐỀ NGHỊ HỖ TRỢ)
-- =====================================================
PRINT N'Đang thêm hồ sơ đề nghị hỗ trợ...';

SET IDENTITY_INSERT Support_Requests ON;
INSERT INTO Support_Requests (RequestId, BeneficiaryId, RequestDate, RequestedAmount, Reason, Status) VALUES
(1, 1, '2025-02-01 09:00:00', 20000000, N'Chi phí điều trị ung thư', N'Đã chi trả'),
(2, 2, '2025-02-15 10:30:00', 15000000, N'Mua xe lăn và thiết bị hỗ trợ', N'Đã chi trả'),
(3, 3, '2025-03-01 14:00:00', 5000000, N'Hỗ trợ sinh hoạt phí 6 tháng', N'Đã phê duyệt'),
(4, 4, '2025-03-10 11:00:00', 10000000, N'Học phí năm học 2025', N'Đã phê duyệt'),
(5, 5, '2025-03-20 15:30:00', 8000000, N'Chi phí chữa bệnh cho mẹ', N'Chờ xét duyệt'),
(6, 6, '2025-04-01 09:00:00', 12000000, N'Sửa chữa nhà cửa sau lũ lụt', N'Chờ xét duyệt'),
(7, 7, '2025-04-15 13:00:00', 6000000, N'Hỗ trợ sinh hoạt và học phí con', N'Chờ xét duyệt'),
(8, 8, '2025-05-01 10:00:00', 18000000, N'Chi phí lọc máu 1 năm', N'Chờ xét duyệt');
SET IDENTITY_INSERT Support_Requests OFF;

PRINT N'✓ Đã thêm 8 hồ sơ đề nghị hỗ trợ';

-- =====================================================
-- INSERT APPROVALS (PHÊ DUYỆT HỒ SƠ)
-- =====================================================
PRINT N'Đang thêm phê duyệt hồ sơ...';

SET IDENTITY_INSERT Approvals ON;
INSERT INTO Approvals (ApprovalId, RequestId, ApprovedBy, ApprovalDate, Result, Note) VALUES
(1, 1, 5, '2025-02-02 10:00:00', N'Phê duyệt', N'Trường hợp khẩn cấp, cần hỗ trợ ngay'),
(2, 2, 5, '2025-02-16 14:00:00', N'Phê duyệt', N'Đã xác minh hoàn cảnh, chấp thuận'),
(3, 3, 8, '2025-03-02 11:00:00', N'Phê duyệt', N'Hỗ trợ người già neo đơn'),
(4, 4, 8, '2025-03-11 09:30:00', N'Phê duyệt', N'Sinh viên có thành tích học tập tốt');
SET IDENTITY_INSERT Approvals OFF;

PRINT N'✓ Đã thêm 4 phê duyệt hồ sơ';

-- =====================================================
-- INSERT EXPENSES (KHOẢN CHI HỖ TRỢ)
-- =====================================================
PRINT N'Đang thêm khoản chi hỗ trợ...';

SET IDENTITY_INSERT Expenses ON;
INSERT INTO Expenses (ExpenseId, RequestId, Amount, ExpenseDate, PaymentMethod, PaidBy) VALUES
(1, 1, 20000000, '2025-02-05 14:00:00', N'Chuyển khoản', 4),
(2, 2, 15000000, '2025-02-20 15:30:00', N'Tiền mặt', 4);
SET IDENTITY_INSERT Expenses OFF;

PRINT N'✓ Đã thêm 2 khoản chi hỗ trợ';

-- =====================================================
-- INSERT LOGS (NHẬT KÝ HỆ THỐNG MẪU)
-- =====================================================
PRINT N'Đang thêm nhật ký hệ thống...';

SET IDENTITY_INSERT Logs ON;
INSERT INTO Logs (LogId, UserId, Action, TableName, ActionTime, OldData, NewData) VALUES
(1, 1, N'Khởi tạo hệ thống', N'System', GETDATE(), NULL, N'Hệ thống được khởi tạo thành công'),
(2, 2, N'Thêm người đóng góp', N'Donors', GETDATE(), NULL, N'Thêm người đóng góp: Nguyễn Văn A'),
(3, 5, N'Phê duyệt hồ sơ', N'Approvals', GETDATE(), NULL, N'Phê duyệt hồ sơ RequestId=1');
SET IDENTITY_INSERT Logs OFF;

PRINT N'✓ Đã thêm nhật ký hệ thống';

PRINT N'';
PRINT N'=====================================================';
PRINT N'✓ HOÀN TẤT THÊM DỮ LIỆU MẪU';
PRINT N'=====================================================';
PRINT N'';
PRINT N'THÔNG TIN ĐĂNG NHẬP:';
PRINT N'---------------------------------------------------';
PRINT N'Admin:       Username: admin       | Password: 123456789';
PRINT N'Staff:       Username: staff1      | Password: 123456789';
PRINT N'Staff:       Username: staff2      | Password: 123456789';
PRINT N'Accountant:  Username: accountant1 | Password: 123456789';
PRINT N'Manager:     Username: manager1    | Password: 123456789';
PRINT N'---------------------------------------------------';
PRINT N'';
GO
