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
(2, N'STAFF', N'Nhân viên quỹ - Thực hiện công việc hỗ trợ'),
(3, N'ACCOUNTANT', N'Khách hàng - Quyên góp và theo dõi hoạt động'),
(4, N'MANAGER', N'Ban quản lý - Tạo công việc, phân công và giám sát');
SET IDENTITY_INSERT Roles OFF;

PRINT N'✓ Đã thêm 4 vai trò';

-- =====================================================
-- INSERT USERS (Password: 123456789 - SHA256)
-- =====================================================
PRINT N'Đang thêm người dùng...';

SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserId, FullName, Username, Password, Email, Phone, Status) VALUES
(1, N'Nguyễn Văn Admin', 'admin', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'admin@quytt.vn', '0901234567', 'Active'),
(2, N'Trần Thị Lan', 'staff1', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'lan.tran@quytt.vn', '0901234568', 'Active'),
(3, N'Lê Văn Minh', 'staff2', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'minh.le@quytt.vn', '0901234569', 'Active'),
(4, N'Phạm Thị Hoa', 'accountant1', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'hoa.pham@quytt.vn', '0901234570', 'Active'),
(5, N'Võ Văn Dũng', 'manager1', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'dung.vo@quytt.vn', '0901234571', 'Active'),
(6, N'Hoàng Thị Mai', 'staff3', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'mai.hoang@quytt.vn', '0901234572', 'Active'),
(7, N'Đặng Văn Hùng', 'accountant2', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'hung.dang@quytt.vn', '0901234573', 'Active'),
(8, N'Bùi Thị Ngọc', 'manager2', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'ngoc.bui@quytt.vn', '0901234574', 'Active'),
(9, N'Nguyễn Văn Tuấn', 'staff4', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'tuan.nguyen@quytt.vn', '0901234575', 'Active'),
(10, N'Phan Thị Linh', 'staff5', '15E2B0D3C33891EBB0F1EF609EC419420C20E320CE94C65FBC8C3312448EB225', 'linh.phan@quytt.vn', '0901234576', 'Active');
SET IDENTITY_INSERT Users OFF;

PRINT N'✓ Đã thêm 10 người dùng';

-- =====================================================
-- INSERT USER_ROLES (PHÂN QUYỀN)
-- =====================================================
PRINT N'Đang phân quyền cho người dùng...';

INSERT INTO User_Roles (UserId, RoleId) VALUES
(1, 1),  -- admin -> ADMIN
(2, 2),  -- staff1 -> STAFF
(3, 2),  -- staff2 -> STAFF
(4, 3),  -- accountant1 -> ACCOUNTANT (Khách hàng)
(5, 4),  -- manager1 -> MANAGER
(6, 2),  -- staff3 -> STAFF
(7, 3),  -- accountant2 -> ACCOUNTANT (Khách hàng)
(8, 4),  -- manager2 -> MANAGER
(9, 2),  -- staff4 -> STAFF
(10, 2); -- staff5 -> STAFF

PRINT N'✓ Đã phân quyền cho 10 người dùng';

-- =====================================================
-- INSERT DONORS (NGƯỜI ĐÓNG GÓP)
-- Bao gồm cả người dùng có tài khoản
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
(10, N'Võ Thị F', N'Cá nhân', N'852 Phan Đình Phùng, Q.5, TP.HCM', '0987654326', 'vothif@gmail.com'),
-- Donor cho user accountant1 (UserId=4)
(11, N'Phạm Thị Hoa', N'Cá nhân', N'789 Trần Hưng Đạo, Q.5, TP.HCM', '0912345678', 'hoa.pham@gmail.com'),
-- Donor cho user accountant2 (UserId=7)
(12, N'Đặng Văn Hùng', N'Cá nhân', N'456 Lý Thường Kiệt, Q.10, TP.HCM', '0923456789', 'hung.dang@gmail.com');
SET IDENTITY_INSERT Donors OFF;

PRINT N'✓ Đã thêm 12 người đóng góp';

-- =====================================================
-- INSERT DONATIONS (KHOẢN QUYÊN GÓP)
-- Bao gồm donations từ user có tài khoản (DonorUserId)
-- =====================================================
PRINT N'Đang thêm khoản quyên góp...';

SET IDENTITY_INSERT Donations ON;
INSERT INTO Donations (DonationId, DonorId, DonorUserId, Amount, DonationDate, Method, ReceivedBy, IsConfirmed) VALUES
-- Donations từ khách vãng lai (không có tài khoản)
(1, 1, NULL, 5000000, '2025-01-15 09:30:00', N'Tiền mặt', 2, 1),
(2, 2, NULL, 50000000, '2025-01-20 10:00:00', N'Chuyển khoản', 2, 1),
(3, 3, NULL, 2000000, '2025-02-05 14:20:00', N'Tiền mặt', 3, 1),
(4, 4, NULL, 100000000, '2025-02-10 11:15:00', N'Chuyển khoản', 2, 1),
(5, 5, NULL, 3000000, '2025-02-25 16:45:00', N'QR Code', 3, 1),
(6, 6, NULL, 75000000, '2025-03-01 09:00:00', N'Chuyển khoản', 2, 1),
(7, 7, NULL, 1500000, '2025-03-10 13:30:00', N'Tiền mặt', 6, 1),
(8, 8, NULL, 4000000, '2025-03-15 10:20:00', N'QR Code', 6, 1),
(9, 9, NULL, 200000000, '2025-03-20 08:00:00', N'Chuyển khoản', 2, 1),
(10, 10, NULL, 2500000, '2025-04-05 15:00:00', N'Tiền mặt', 3, 1),
(11, 1, NULL, 10000000, '2025-04-15 11:00:00', N'Chuyển khoản', 2, 1),
(12, 3, NULL, 3500000, '2025-05-01 14:00:00', N'QR Code', 6, 1),
-- Donations từ user accountant1 (UserId=4, DonorId=11)
(13, 11, 4, 500000, '2025-02-01 08:30:00', N'Chuyển khoản', NULL, 1),
(14, 11, 4, 1000000, '2025-02-02 14:15:00', N'Chuyển khoản', NULL, 1),
(15, 11, 4, 2000000, '2025-02-03 10:00:00', N'Chuyển khoản', NULL, 1),
-- Donations từ user accountant2 (UserId=7, DonorId=12)
(16, 12, 7, 750000, '2025-02-01 09:00:00', N'Chuyển khoản', NULL, 1),
(17, 12, 7, 1500000, '2025-02-03 16:30:00', N'Chuyển khoản', NULL, 1),
-- ===== DONATIONS 7 NGÀY GẦN ĐÂY (29/01/2026 - 04/02/2026) =====
(18, 1, NULL, 8000000, '2026-01-29 09:00:00', N'Chuyển khoản', 2, 1),
(19, 2, NULL, 25000000, '2026-01-29 14:30:00', N'Chuyển khoản', 2, 1),
(20, 3, NULL, 3500000, '2026-01-30 10:15:00', N'Tiền mặt', 3, 1),
(21, 5, NULL, 5000000, '2026-01-30 16:00:00', N'QR Code', 6, 1),
(22, 11, 4, 2000000, '2026-01-31 08:30:00', N'Chuyển khoản', NULL, 1),
(23, 6, NULL, 15000000, '2026-01-31 11:00:00', N'Chuyển khoản', 2, 1),
(24, 7, NULL, 4500000, '2026-02-01 09:45:00', N'Tiền mặt', 3, 1),
(25, 12, 7, 3000000, '2026-02-01 15:00:00', N'Chuyển khoản', NULL, 1),
(26, 8, NULL, 7500000, '2026-02-02 10:30:00', N'QR Code', 6, 1),
(27, 9, NULL, 50000000, '2026-02-02 14:00:00', N'Chuyển khoản', 2, 1),
(28, 10, NULL, 2500000, '2026-02-03 09:00:00', N'Tiền mặt', 3, 1),
(29, 4, NULL, 30000000, '2026-02-03 13:30:00', N'Chuyển khoản', 2, 1),
(30, 11, 4, 5000000, '2026-02-04 08:00:00', N'Chuyển khoản', NULL, 1),
(31, 1, NULL, 10000000, '2026-02-04 11:30:00', N'QR Code', 6, 1);
SET IDENTITY_INSERT Donations OFF;

PRINT N'✓ Đã thêm 31 khoản quyên góp';

-- =====================================================
-- INSERT FUNDS (QUỸ TIỀN)
-- Tổng = 456,500,000 + 3,500,000 + 2,250,000 = 462,250,000
-- =====================================================
PRINT N'Đang khởi tạo quỹ...';

SET IDENTITY_INSERT Funds ON;
INSERT INTO Funds (FundId, FundName, Balance, LastUpdated) VALUES
(1, N'Quỹ Tình Thương', 462250000, GETDATE());
SET IDENTITY_INSERT Funds OFF;

PRINT N'✓ Đã khởi tạo quỹ với số dư: 462,250,000 VNĐ';

-- =====================================================
-- INSERT BENEFICIARIES (ĐỐI TƯỢNG THỤ HƯỞNG)
-- =====================================================
PRINT N'Đang thêm đối tượng thụ hưởng...';

SET IDENTITY_INSERT Beneficiaries ON;
INSERT INTO Beneficiaries (BeneficiaryId, FullName, BeneficiaryType, Address, Description, Status, CreatedBy, CreatedAt) VALUES
(1, N'Nguyễn Thị Mai', N'Bệnh nhân hiểm nghèo', N'123 Xã Tân Lập, Huyện Bình Chánh, TP.HCM', N'Bệnh nhân ung thư giai đoạn cuối, gia đình khó khăn', N'Đã duyệt', 1, '2025-01-15 09:00:00'),
(2, N'Trần Văn Bình', N'Người khuyết tật', N'456 Xã Phước Kiển, Huyện Nhà Bè, TP.HCM', N'Khuyết tật bẩm sinh, không có khả năng lao động', N'Đã duyệt', 1, '2025-01-16 10:00:00'),
(3, N'Lê Thị Hoa', N'Người già neo đơn', N'789 Xã Long Trường, Quận 9, TP.HCM', N'Cụ bà 78 tuổi sống một mình, không con cái', N'Đã duyệt', 5, '2025-01-20 11:00:00'),
(4, N'Phạm Văn Tùng', N'Học sinh/Sinh viên nghèo', N'321 Xã Bình Hưng, Huyện Bình Chánh, TP.HCM', N'Sinh viên mồ côi, học giỏi nhưng hoàn cảnh khó khăn', N'Đã duyệt', 5, '2025-01-25 14:00:00'),
(5, N'Hoàng Thị Lan', N'Trẻ em khó khăn', N'654 Xã Phú Xuân, Huyện Nhà Bè, TP.HCM', N'Trẻ mồ côi cha, mẹ bệnh nặng không có khả năng nuôi con', N'Đã duyệt', 8, '2025-02-01 09:00:00'),
(6, N'Võ Văn Đức', N'Nạn nhân thiên tai', N'147 Xã Tân Nhựt, Huyện Bình Chánh, TP.HCM', N'Gia đình bị thiệt hại nặng do lũ lụt', N'Đã duyệt', 8, '2025-02-10 10:00:00'),
(7, N'Đặng Thị Ngọc', N'Người nghèo', N'258 Xã Phước Lộc, Huyện Nhà Bè, TP.HCM', N'Gia đình nghèo 5 người, thu nhập thấp', N'Đã duyệt', 1, '2025-03-01 11:00:00'),
(8, N'Bùi Văn Hải', N'Bệnh nhân hiểm nghèo', N'369 Xã Long Phước, Quận 9, TP.HCM', N'Bệnh thận mãn tính, cần lọc máu thường xuyên', N'Đã duyệt', 5, '2025-03-15 14:00:00'),
-- Đối tượng do Staff thêm (chờ duyệt)
(9, N'Trần Thị Hương', N'Người nghèo', N'111 Xã Tân Kiên, Huyện Bình Chánh, TP.HCM', N'Mẹ đơn thân nuôi 3 con nhỏ, không có việc làm ổn định', N'Chờ duyệt', 2, '2026-02-01 08:30:00'),
(10, N'Lý Văn Phú', N'Bệnh nhân hiểm nghèo', N'222 Xã An Phú Tây, Huyện Bình Chánh, TP.HCM', N'Bệnh tim bẩm sinh, cần phẫu thuật gấp', N'Chờ duyệt', 3, '2026-02-02 09:00:00'),
(11, N'Ngô Thị Thanh', N'Người khuyết tật', N'333 Xã Quy Đức, Huyện Bình Chánh, TP.HCM', N'Mù bẩm sinh, sống một mình', N'Chờ duyệt', 6, '2026-02-03 10:00:00');
SET IDENTITY_INSERT Beneficiaries OFF;

PRINT N'✓ Đã thêm 11 đối tượng thụ hưởng';

-- =====================================================
-- INSERT SUPPORT_REQUESTS (HỒ SƠ ĐỀ NGHỊ HỖ TRỢ)
-- =====================================================
PRINT N'Đang thêm hồ sơ đề nghị hỗ trợ...';

SET IDENTITY_INSERT Support_Requests ON;
INSERT INTO Support_Requests (RequestId, BeneficiaryId, RequestDate, RequestedAmount, SupportIssue, Reason, Status, CreatedBy) VALUES
(1, 1, '2025-02-01 09:00:00', 20000000, N'Cần chi phí điều trị ung thư', N'Chi phí điều trị ung thư', N'Đã chi trả', 2),
(2, 2, '2025-02-15 10:30:00', 15000000, N'Cần xe lăn và thiết bị hỗ trợ', N'Mua xe lăn và thiết bị hỗ trợ', N'Đã chi trả', 2),
(3, 3, '2025-03-01 14:00:00', 5000000, N'Không có tiền sinh hoạt', N'Hỗ trợ sinh hoạt phí 6 tháng', N'Đã phê duyệt', 3),
(4, 4, '2025-03-10 11:00:00', 10000000, N'Không đủ tiền đóng học phí', N'Học phí năm học 2025', N'Đã phê duyệt', 3),
(5, 5, '2025-03-20 15:30:00', NULL, N'Mẹ bị bệnh nặng cần chữa trị', N'Chi phí chữa bệnh cho mẹ', N'Chờ xét duyệt', 2),
(6, 6, '2025-04-01 09:00:00', NULL, N'Nhà bị hư hỏng nặng sau lũ lụt', N'Sửa chữa nhà cửa sau lũ lụt', N'Chờ xét duyệt', 6),
(7, 7, '2025-04-15 13:00:00', NULL, N'Gia đình khó khăn, con cần đi học', N'Hỗ trợ sinh hoạt và học phí con', N'Chờ xét duyệt', 2),
(8, 8, '2025-05-01 10:00:00', NULL, N'Cần lọc máu thường xuyên', N'Chi phí lọc máu 1 năm', N'Chờ xét duyệt', 3),
-- ===== SUPPORT REQUESTS MỚI (Đa dạng trạng thái) =====
(9, 1, '2026-01-20 09:00:00', 15000000, N'Cần thuốc điều trị tiếp theo', N'Chi phí thuốc điều trị tiếp theo', N'Đã phê duyệt', 2),
(10, 2, '2026-01-22 10:00:00', 8000000, N'Cần thêm thiết bị phục hồi chức năng', N'Mua thêm thiết bị phục hồi chức năng', N'Đã phê duyệt', 3),
(11, 3, '2026-01-25 11:00:00', NULL, N'Cần hỗ trợ tiền điện nước', N'Hỗ trợ tiền điện nước 3 tháng', N'Từ chối', 6),
(12, 4, '2026-01-28 14:00:00', NULL, N'Cần sách vở và đồ dùng học tập', N'Mua sách vở và đồ dùng học tập', N'Chờ xét duyệt', 2),
(13, 5, '2026-01-30 09:30:00', 10000000, N'Mẹ cần phẫu thuật khẩn cấp', N'Phẫu thuật cho mẹ', N'Đã phê duyệt', 3),
(14, 6, '2026-02-01 10:00:00', NULL, N'Cần vật liệu xây dựng sửa nhà', N'Mua vật liệu xây dựng', N'Chờ xét duyệt', 6),
(15, 7, '2026-02-02 11:00:00', NULL, N'Cần hỗ trợ học phí học kỳ 2', N'Học phí học kỳ 2 cho con', N'Từ chối', 2),
(16, 8, '2026-02-03 09:00:00', NULL, N'Cần xét nghiệm và mua thuốc', N'Chi phí xét nghiệm và thuốc', N'Chờ xét duyệt', 3);
SET IDENTITY_INSERT Support_Requests OFF;

PRINT N'✓ Đã thêm 16 hồ sơ đề nghị hỗ trợ';

-- =====================================================
-- INSERT APPROVALS (PHÊ DUYỆT HỒ SƠ)
-- =====================================================
PRINT N'Đang thêm phê duyệt hồ sơ...';

SET IDENTITY_INSERT Approvals ON;
INSERT INTO Approvals (ApprovalId, RequestId, ApprovedBy, ApprovalDate, Result, Note) VALUES
(1, 1, 5, '2025-02-02 10:00:00', N'Phê duyệt', N'Trường hợp khẩn cấp, cần hỗ trợ ngay'),
(2, 2, 5, '2025-02-16 14:00:00', N'Phê duyệt', N'Đã xác minh hoàn cảnh, chấp thuận'),
(3, 3, 8, '2025-03-02 11:00:00', N'Phê duyệt', N'Hỗ trợ người già neo đơn'),
(4, 4, 8, '2025-03-11 09:30:00', N'Phê duyệt', N'Sinh viên có thành tích học tập tốt'),
-- ===== APPROVALS MỚI =====
(5, 9, 5, '2026-01-21 10:00:00', N'Phê duyệt', N'Tiếp tục hỗ trợ điều trị bệnh nhân'),
(6, 10, 5, '2026-01-23 14:00:00', N'Phê duyệt', N'Đã xác minh nhu cầu thiết bị'),
(7, 11, 8, '2026-01-26 11:00:00', N'Từ chối', N'Không đủ điều kiện, hồ sơ chưa đầy đủ'),
(8, 13, 5, '2026-01-31 09:00:00', N'Phê duyệt', N'Trường hợp khẩn cấp, cần phẫu thuật'),
(9, 15, 8, '2026-02-03 10:00:00', N'Từ chối', N'Đã hỗ trợ trong đợt trước, chờ đợt tiếp theo');
SET IDENTITY_INSERT Approvals OFF;

PRINT N'✓ Đã thêm 9 phê duyệt hồ sơ';

-- =====================================================
-- INSERT EXPENSES (KHOẢN CHI HỖ TRỢ)
-- =====================================================
PRINT N'Đang thêm khoản chi hỗ trợ...';

SET IDENTITY_INSERT Expenses ON;
INSERT INTO Expenses (ExpenseId, RequestId, Amount, ExpenseDate, PaymentMethod, PaidBy) VALUES
(1, 1, 20000000, '2025-02-05 14:00:00', N'Chuyển khoản', 4),
(2, 2, 15000000, '2025-02-20 15:30:00', N'Tiền mặt', 4),
-- ===== EXPENSES 7 NGÀY GẦN ĐÂY (29/01/2026 - 04/02/2026) =====
(3, 3, 5000000, '2026-01-29 10:00:00', N'Tiền mặt', 4),
(4, 4, 12000000, '2026-01-30 14:30:00', N'Chuyển khoản', 4),
(5, 5, 8000000, '2026-01-31 09:00:00', N'Chuyển khoản', 7),
(6, 6, 6000000, '2026-02-01 11:00:00', N'Tiền mặt', 4),
(7, 7, 4000000, '2026-02-02 15:00:00', N'Chuyển khoản', 7);
SET IDENTITY_INSERT Expenses OFF;

PRINT N'✓ Đã thêm 7 khoản chi hỗ trợ';

-- =====================================================
-- INSERT SUPPORT_TASKS (CÔNG VIỆC HỖ TRỢ)
-- Manager đã tạo và phân công cho Staff
-- =====================================================
PRINT N'Đang thêm công việc hỗ trợ...';

SET IDENTITY_INSERT Support_Tasks ON;
INSERT INTO Support_Tasks (TaskId, RequestId, AssignedStaffId, DonorUserId, AssignedBy, AssignedAt, Amount, AdditionalAmount, Status, StartedAt, StaffNote, StaffCompletedAt, ManagerNote, ManagerVerifiedAt, SupportRequestType, SupportRequestReason, SupportRequestAmount, SupportRequestAt, SupportResponseNote, SupportResponseAt, CreatedAt, UpdatedAt) VALUES
-- Task đã hoàn thành
(1, 1, 2, 4, 5, '2025-02-02 10:30:00', 20000000, 0, N'Hoàn thành', '2025-02-03 08:00:00', N'Đã trao tiền trực tiếp cho gia đình bệnh nhân tại nhà. Gia đình rất biết ơn sự hỗ trợ của quỹ.', '2025-02-05 14:00:00', N'Hỗ trợ chi phí điều trị ung thư', '2025-02-05 15:00:00', NULL, NULL, NULL, NULL, NULL, NULL, '2025-02-02 10:30:00', '2025-02-05 15:00:00'),
(2, 2, 3, 7, 5, '2025-02-16 14:30:00', 15000000, 0, N'Hoàn thành', '2025-02-17 09:00:00', N'Đã mua xe lăn chất lượng cao và giao tận nơi. Người nhận rất vui mừng và xúc động.', '2025-02-20 15:30:00', N'Mua xe lăn cho người khuyết tật', '2025-02-20 16:00:00', NULL, NULL, NULL, NULL, NULL, NULL, '2025-02-16 14:30:00', '2025-02-20 16:00:00'),
-- Task đang thực hiện
(3, 3, 2, 4, 5, '2025-03-02 11:30:00', 5000000, 0, N'Đang thực hiện', '2025-03-03 08:00:00', NULL, NULL, N'Hỗ trợ sinh hoạt phí 6 tháng', NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2025-03-02 11:30:00', '2025-03-03 08:00:00'),
-- Task yêu cầu hỗ trợ (Staff cần thêm tiền)
(4, 4, 6, 7, 5, '2025-03-11 10:00:00', 10000000, 2000000, N'Đang thực hiện', '2025-03-12 08:00:00', N'Đang liên hệ với trường đại học để chuyển học phí', NULL, N'Hỗ trợ học phí năm học 2025', NULL, N'Tiền', N'Cần thêm 2 triệu để đóng phí ký túc xá cho sinh viên', 2000000, '2025-03-13 10:00:00', N'Đã duyệt thêm 2 triệu. Hãy hoàn thành sớm nhé!', '2025-03-13 11:00:00', '2025-03-11 10:00:00', '2025-03-13 11:00:00'),
-- Task chờ thực hiện
(5, 5, 2, NULL, 5, '2025-03-21 09:00:00', 8000000, 0, N'Chờ thực hiện', NULL, NULL, NULL, N'Sửa nhà cho gia đình khó khăn', NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2025-03-21 09:00:00', '2025-03-21 09:00:00'),
(6, 6, 3, NULL, 8, '2025-04-02 10:00:00', 12000000, 0, N'Chờ thực hiện', NULL, NULL, NULL, N'Chi phí tang lễ', NULL, NULL, NULL, NULL, NULL, NULL, NULL, '2025-04-02 10:00:00', '2025-04-02 10:00:00'),
-- Task hoàn thành khác
(7, 7, 6, 4, 8, '2025-04-15 14:00:00', 6000000, 0, N'Hoàn thành', '2025-04-16 08:00:00', N'Đã hỗ trợ tiền sinh hoạt và học phí cho 2 con của gia đình. Các cháu học giỏi và ngoan ngoãn.', '2025-04-18 16:00:00', N'Hỗ trợ trẻ em mồ côi', '2025-04-18 17:00:00', NULL, NULL, NULL, NULL, NULL, NULL, '2025-04-15 14:00:00', '2025-04-18 17:00:00'),
(8, 8, 2, 7, 5, '2025-05-01 11:00:00', 18000000, 0, N'Hoàn thành', '2025-05-02 09:00:00', N'Đã chuyển khoản cho bệnh viện để thanh toán chi phí lọc máu 1 năm cho bệnh nhân.', '2025-05-03 11:00:00', N'Chi phí lọc máu 1 năm', '2025-05-03 12:00:00', NULL, NULL, NULL, NULL, NULL, NULL, '2025-05-01 11:00:00', '2025-05-03 12:00:00');
SET IDENTITY_INSERT Support_Tasks OFF;

PRINT N'✓ Đã thêm 8 công việc hỗ trợ';

-- =====================================================
-- INSERT NOTIFICATIONS (THÔNG BÁO)
-- =====================================================
PRINT N'Đang thêm thông báo...';

SET IDENTITY_INSERT Notifications ON;
INSERT INTO Notifications (NotificationId, UserId, Title, Message, Type, RelatedTaskId, IsRead, CreatedAt) VALUES
-- Thông báo cho Staff về công việc mới
(1, 2, N'Công việc mới được giao', N'Bạn được phân công hỗ trợ bệnh nhân Nguyễn Thị Mai - 20,000,000 VNĐ. Đây là trường hợp khẩn cấp, vui lòng thực hiện sớm.', N'Công việc mới', 1, 1, '2025-02-02 10:30:00'),
(2, 3, N'Công việc mới được giao', N'Bạn được phân công hỗ trợ người khuyết tật Trần Văn Bình - 15,000,000 VNĐ (mua xe lăn và thiết bị hỗ trợ)', N'Công việc mới', 2, 1, '2025-02-16 14:30:00'),
(3, 2, N'Công việc mới được giao', N'Bạn được phân công hỗ trợ cụ bà Lê Thị Hoa - 5,000,000 VNĐ sinh hoạt phí 6 tháng', N'Công việc mới', 3, 1, '2025-03-02 11:30:00'),
(4, 6, N'Công việc mới được giao', N'Bạn được phân công hỗ trợ sinh viên Phạm Văn Tùng - 10,000,000 VNĐ học phí năm học 2025', N'Công việc mới', 4, 1, '2025-03-11 10:00:00'),
(5, 2, N'Công việc mới được giao', N'Bạn được phân công hỗ trợ bé Hoàng Thị Lan - 8,000,000 VNĐ chi phí chữa bệnh cho mẹ', N'Công việc mới', 5, 0, '2025-03-21 09:00:00'),
(6, 3, N'Công việc mới được giao', N'Bạn được phân công hỗ trợ gia đình Võ Văn Đức - 12,000,000 VNĐ sửa chữa nhà sau lũ', N'Công việc mới', 6, 0, '2025-04-02 10:00:00'),
(7, 6, N'Công việc mới được giao', N'Bạn được phân công hỗ trợ gia đình Đặng Thị Ngọc - 6,000,000 VNĐ sinh hoạt và học phí con', N'Công việc mới', 7, 1, '2025-04-15 14:00:00'),
(8, 2, N'Công việc mới được giao', N'Bạn được phân công hỗ trợ bệnh nhân Bùi Văn Hải - 18,000,000 VNĐ chi phí lọc máu 1 năm', N'Công việc mới', 8, 1, '2025-05-01 11:00:00'),

-- Thông báo Manager phản hồi yêu cầu hỗ trợ
(9, 6, N'Yêu cầu hỗ trợ đã được duyệt', N'Manager đã duyệt yêu cầu bổ sung 2,000,000 VNĐ cho công việc hỗ trợ sinh viên Phạm Văn Tùng. Ghi chú: Đã duyệt thêm 2 triệu. Hãy hoàn thành sớm nhé!', N'Phản hồi yêu cầu', 4, 1, '2025-03-12 10:00:00'),

-- Thông báo cho TẤT CẢ khách hàng (Accountant) về hỗ trợ hoàn thành
(10, 4, N'🎉 Hoạt động hỗ trợ hoàn thành', N'Quỹ đã hoàn thành hỗ trợ bệnh nhân Nguyễn Thị Mai với số tiền 20,000,000 VNĐ. Nhân viên ghi chú: Đã trao tiền trực tiếp cho gia đình bệnh nhân tại nhà.', N'Hoàn thành', 1, 1, '2025-02-05 14:00:00'),
(11, 7, N'🎉 Hoạt động hỗ trợ hoàn thành', N'Quỹ đã hoàn thành hỗ trợ bệnh nhân Nguyễn Thị Mai với số tiền 20,000,000 VNĐ. Nhân viên ghi chú: Đã trao tiền trực tiếp cho gia đình bệnh nhân tại nhà.', N'Hoàn thành', 1, 0, '2025-02-05 14:00:00'),
(12, 4, N'🎉 Hoạt động hỗ trợ hoàn thành', N'Quỹ đã hoàn thành hỗ trợ người khuyết tật Trần Văn Bình với số tiền 15,000,000 VNĐ. Nhân viên ghi chú: Đã mua xe lăn chất lượng cao và giao tận nơi.', N'Hoàn thành', 2, 1, '2025-02-20 15:30:00'),
(13, 7, N'🎉 Hoạt động hỗ trợ hoàn thành', N'Quỹ đã hoàn thành hỗ trợ người khuyết tật Trần Văn Bình với số tiền 15,000,000 VNĐ. Nhân viên ghi chú: Đã mua xe lăn chất lượng cao và giao tận nơi.', N'Hoàn thành', 2, 1, '2025-02-20 15:30:00'),
(14, 4, N'🎉 Hoạt động hỗ trợ hoàn thành', N'Quỹ đã hoàn thành hỗ trợ gia đình Đặng Thị Ngọc với số tiền 6,000,000 VNĐ. Nhân viên ghi chú: Đã hỗ trợ tiền sinh hoạt và học phí cho 2 con của gia đình.', N'Hoàn thành', 7, 0, '2025-04-18 16:00:00'),
(15, 7, N'🎉 Hoạt động hỗ trợ hoàn thành', N'Quỹ đã hoàn thành hỗ trợ gia đình Đặng Thị Ngọc với số tiền 6,000,000 VNĐ. Nhân viên ghi chú: Đã hỗ trợ tiền sinh hoạt và học phí cho 2 con của gia đình.', N'Hoàn thành', 7, 0, '2025-04-18 16:00:00'),
(16, 4, N'🎉 Hoạt động hỗ trợ hoàn thành', N'Quỹ đã hoàn thành hỗ trợ bệnh nhân Bùi Văn Hải với số tiền 18,000,000 VNĐ. Nhân viên ghi chú: Đã chuyển khoản cho bệnh viện để thanh toán chi phí lọc máu 1 năm.', N'Hoàn thành', 8, 0, '2025-05-03 11:00:00'),
(17, 7, N'🎉 Hoạt động hỗ trợ hoàn thành', N'Quỹ đã hoàn thành hỗ trợ bệnh nhân Bùi Văn Hải với số tiền 18,000,000 VNĐ. Nhân viên ghi chú: Đã chuyển khoản cho bệnh viện để thanh toán chi phí lọc máu 1 năm.', N'Hoàn thành', 8, 0, '2025-05-03 11:00:00'),

-- Thông báo cho Manager về yêu cầu hỗ trợ từ Staff
(18, 5, N'🔔 Nhân viên yêu cầu hỗ trợ', N'Nhân viên Hoàng Thị Mai yêu cầu bổ sung 2,000,000 VNĐ cho công việc hỗ trợ sinh viên Phạm Văn Tùng. Lý do: Cần thêm tiền để đóng phí ký túc xá.', N'Yêu cầu hỗ trợ', 4, 1, '2025-03-12 09:00:00'),
(19, 8, N'🔔 Nhân viên yêu cầu hỗ trợ', N'Nhân viên Hoàng Thị Mai yêu cầu bổ sung 2,000,000 VNĐ cho công việc hỗ trợ sinh viên Phạm Văn Tùng. Lý do: Cần thêm tiền để đóng phí ký túc xá.', N'Yêu cầu hỗ trợ', 4, 1, '2025-03-12 09:00:00');
SET IDENTITY_INSERT Notifications OFF;

PRINT N'✓ Đã thêm 19 thông báo';

-- =====================================================
-- INSERT COMPLAINTS (PHẢN ÁNH TỪ KHÁCH HÀNG)
-- =====================================================
PRINT N'Đang thêm phản ánh từ khách hàng...';

SET IDENTITY_INSERT Complaints ON;
INSERT INTO Complaints (ComplaintId, TaskId, UserId, Content, Status, ResponseContent, ResponseAt, CreatedAt) VALUES
-- Phản ánh đã được phản hồi
(1, 1, 4, N'Tôi rất hài lòng với sự hỗ trợ kịp thời của quỹ. Gia đình bệnh nhân đã nhận được tiền đúng hạn và rất biết ơn. Cảm ơn quỹ rất nhiều!', N'Đã phản hồi', N'Cảm ơn bạn đã gửi phản hồi tích cực! Chúng tôi rất vui vì đã giúp được gia đình bệnh nhân. Đây là động lực để quỹ tiếp tục hoạt động.', '2025-02-10 10:00:00', '2025-02-08 15:00:00'),
(2, 2, 7, N'Xe lăn chất lượng rất tốt, người nhận rất hài lòng. Tuy nhiên tôi muốn hỏi có hỗ trợ thêm các thiết bị phục hồi chức năng không?', N'Đã phản hồi', N'Cảm ơn phản hồi của bạn! Về việc hỗ trợ thiết bị phục hồi chức năng, quỹ sẽ xem xét trong các đợt hỗ trợ tiếp theo. Vui lòng theo dõi thông tin từ quỹ.', '2025-02-25 14:00:00', '2025-02-23 11:00:00'),

-- Phản ánh chờ xử lý
(3, 7, 4, N'Rất cảm ơn quỹ đã hỗ trợ gia đình khó khăn. Các cháu nhỏ rất vui khi được tiếp tục đi học. Mong quỹ ngày càng phát triển!', N'Chờ xử lý', NULL, NULL, '2025-04-21 09:30:00'),
(4, 8, 7, N'Tôi thấy việc hỗ trợ chi phí lọc máu rất ý nghĩa. Bệnh nhân đã có thể yên tâm điều trị cả năm mà không lo về tài chính. Cảm ơn quỹ!', N'Chờ xử lý', NULL, NULL, '2025-05-06 16:00:00');
SET IDENTITY_INSERT Complaints OFF;

PRINT N'✓ Đã thêm 4 phản ánh từ khách hàng';

-- =====================================================
-- INSERT LOGS (NHẬT KÝ HỆ THỐNG MẪU)
-- =====================================================
PRINT N'Đang thêm nhật ký hệ thống...';

SET IDENTITY_INSERT Logs ON;
INSERT INTO Logs (LogId, UserId, Action, TableName, ActionTime, OldData, NewData) VALUES
(1, 1, N'Khởi tạo hệ thống', N'System', '2025-01-01 00:00:00', NULL, N'Hệ thống Quỹ Tình Thương được khởi tạo thành công'),
(2, 5, N'Tạo công việc hỗ trợ', N'Support_Tasks', '2025-02-02 10:30:00', NULL, N'Tạo task #1 - Hỗ trợ bệnh nhân Nguyễn Thị Mai 20,000,000 VNĐ, giao cho staff1'),
(3, 2, N'Bắt đầu thực hiện công việc', N'Support_Tasks', '2025-02-03 08:00:00', N'Status: Chờ thực hiện', N'Status: Đang thực hiện. Staff bắt đầu thực hiện task #1'),
(4, 2, N'Hoàn thành công việc', N'Support_Tasks', '2025-02-05 14:00:00', N'Status: Đang thực hiện', N'Status: Hoàn thành. Staff hoàn thành task #1'),
(5, 4, N'Quyên góp', N'Donations', '2025-02-01 08:30:00', NULL, N'Khách hàng accountant1 quyên góp 500,000 VNĐ qua chuyển khoản'),
(6, 4, N'Quyên góp', N'Donations', '2025-02-02 14:15:00', NULL, N'Khách hàng accountant1 quyên góp 1,000,000 VNĐ qua chuyển khoản'),
(7, 7, N'Quyên góp', N'Donations', '2025-02-01 09:00:00', NULL, N'Khách hàng accountant2 quyên góp 750,000 VNĐ qua chuyển khoản'),
(8, 5, N'Tạo công việc hỗ trợ', N'Support_Tasks', '2025-02-16 14:30:00', NULL, N'Tạo task #2 - Hỗ trợ người khuyết tật Trần Văn Bình 15,000,000 VNĐ'),
(9, 3, N'Hoàn thành công việc', N'Support_Tasks', '2025-02-20 15:30:00', N'Status: Đang thực hiện', N'Status: Hoàn thành. Đã mua và giao xe lăn'),
(10, 6, N'Yêu cầu hỗ trợ', N'Support_Tasks', '2025-03-12 09:00:00', NULL, N'Staff yêu cầu bổ sung 2,000,000 VNĐ cho task #4'),
(11, 5, N'Duyệt yêu cầu hỗ trợ', N'Support_Tasks', '2025-03-12 10:00:00', N'AdditionalAmount: 0', N'AdditionalAmount: 2,000,000. Đã duyệt thêm tiền'),
(12, 4, N'Gửi phản ánh', N'Complaints', '2025-02-08 15:00:00', NULL, N'Khách hàng gửi phản ánh tích cực về task #1'),
(13, 5, N'Phản hồi phản ánh', N'Complaints', '2025-02-10 10:00:00', N'Status: Chờ xử lý', N'Status: Đã phản hồi. Manager phản hồi phản ánh #1'),
(14, 2, N'Hoàn thành công việc', N'Support_Tasks', '2025-05-03 11:00:00', N'Status: Đang thực hiện', N'Status: Hoàn thành. Đã chuyển khoản chi phí lọc máu'),
(15, 1, N'Đăng nhập hệ thống', N'Users', GETDATE(), NULL, N'Admin đăng nhập hệ thống');
SET IDENTITY_INSERT Logs OFF;

PRINT N'✓ Đã thêm 15 nhật ký hệ thống';

-- =====================================================
-- BẢNG SUPPORT_HELPERS (NGƯỜI HỖ TRỢ)
-- Không có dữ liệu mẫu, bảng này sẽ được điền khi 
-- Manager gửi lời mời hỗ trợ cho Staff
-- =====================================================
PRINT N'✓ Bảng Support_Helpers trống (sẽ được điền khi sử dụng)';

PRINT N'=====================================================';
PRINT N'HOÀN TẤT THÊM DỮ LIỆU MẪU';
PRINT N'=====================================================';
GO
