# 🤝 Quỹ Tình Thương - Hệ Thống Quản Lý Quỹ Từ Thiện

## 📋 Giới Thiệu

**Quỹ Tình Thương** là ứng dụng web quản lý quỹ từ thiện được xây dựng trên nền tảng ASP.NET Core 8.0 MVC với SQL Server. Hệ thống giúp quản lý các hoạt động quyên góp, phân bổ nguồn lực hỗ trợ và theo dõi tiến độ giải ngân một cách minh bạch.

## 🛠️ Công Nghệ Sử Dụng

- **Backend**: ASP.NET Core 8.0 MVC
- **ORM**: Entity Framework Core
- **Database**: SQL Server (localhost\SQLEXPRESS)
- **Frontend**: Razor Views, Bootstrap 5, CSS tùy chỉnh
- **Authentication**: Session-based authentication

## 👥 Phân Quyền Người Dùng

Hệ thống có 4 vai trò với các chức năng khác nhau:

### 1. 🔐 Admin (Quản trị viên)
- Quản lý người dùng (thêm/sửa/xóa/khóa tài khoản)
- Phân quyền người dùng
- Xem thống kê tổng quan hệ thống
- Quản lý dữ liệu master

### 2. 📊 Manager (Quản lý)
- Tạo và phân công công việc hỗ trợ cho Staff
- Duyệt/Từ chối yêu cầu hỗ trợ thêm từ Staff
- Xử lý phản ánh từ khách hàng
- Xem báo cáo thống kê
- Quản lý thông báo

### 3. 👷 Staff (Nhân viên)
- Nhận và thực hiện công việc hỗ trợ
- Yêu cầu hỗ trợ thêm (tiền/nhân lực) khi cần
- Cập nhật tiến độ và hoàn thành công việc
- Quản lý danh sách người thụ hưởng
- Quản lý thông tin nhà tài trợ

### 4. 💝 Accountant/Khách hàng (Nhà tài trợ)
- Quyên góp tiền vào quỹ
- Xem lịch sử quyên góp của bản thân
- Theo dõi các hoạt động hỗ trợ đã hoàn thành
- Nhận thông báo về hoạt động của quỹ
- Gửi phản hồi/phản ánh về dịch vụ hỗ trợ

## 📁 Cấu Trúc Dự Án

```
WebApplication1/
├── Controllers/           # Các controller xử lý logic
│   ├── AccountController.cs      # Đăng nhập/Đăng ký
│   ├── AdminController.cs        # Quản trị hệ thống
│   ├── ManagerController.cs      # Quản lý công việc
│   ├── StaffController.cs        # Nhân viên thực hiện
│   └── AccountantController.cs   # Khách hàng/Nhà tài trợ
├── Models/
│   ├── Entities/          # Entity classes (mapping với DB)
│   │   ├── User.cs, Role.cs, UserRole.cs
│   │   ├── Donor.cs, Donation.cs, Fund.cs
│   │   ├── Beneficiary.cs, SupportRequest.cs
│   │   ├── SupportTask.cs, Approval.cs, Expense.cs
│   │   ├── Notification.cs, Complaint.cs, Log.cs
│   └── ViewModels/        # View models cho các trang
├── Views/                 # Razor views
│   ├── Account/           # Đăng nhập, đăng ký
│   ├── Admin/             # Giao diện admin
│   ├── Manager/           # Giao diện quản lý
│   ├── Staff/             # Giao diện nhân viên
│   ├── Accountant/        # Giao diện khách hàng
│   └── Shared/            # Layout, sidebar chung
├── lib/                   # SQL Scripts
│   ├── 01_CreateTables.sql      # Tạo cấu trúc database
│   ├── 02_InsertData.sql        # Dữ liệu mẫu
│   └── 03_CreateSchedule.sql    # Triggers & Stored Procedures
├── wwwroot/               # Static files (CSS, JS)
└── Utils/                 # Helper classes
```

## 🗄️ Cơ Sở Dữ Liệu

### Các bảng chính:
| Bảng | Mô tả |
|------|-------|
| `Users` | Thông tin người dùng |
| `Roles` | Vai trò (Admin, Manager, Staff, Accountant) |
| `User_Roles` | Phân quyền người dùng |
| `Donors` | Thông tin nhà tài trợ |
| `Donations` | Các khoản quyên góp |
| `Funds` | Số dư quỹ |
| `Beneficiaries` | Người thụ hưởng |
| `Support_Requests` | Hồ sơ đề nghị hỗ trợ |
| `Support_Tasks` | Công việc hỗ trợ được phân công |
| `Approvals` | Phê duyệt hồ sơ |
| `Expenses` | Chi phí giải ngân |
| `Notifications` | Thông báo |
| `Complaints` | Phản ánh từ khách hàng |
| `Logs` | Nhật ký hệ thống |

## 🚀 Hướng Dẫn Cài Đặt

### Yêu cầu:
- .NET 8.0 SDK
- SQL Server (Express hoặc cao hơn)
- Visual Studio 2022 hoặc VS Code

### Các bước cài đặt:

1. **Clone repository**
   ```bash
   git clone <repository-url>
   cd WebApplication1
   ```

2. **Tạo database** (chạy theo thứ tự):
   ```bash
   sqlcmd -S localhost\SQLEXPRESS -E -i "lib/01_CreateTables.sql"
   sqlcmd -S localhost\SQLEXPRESS -E -i "lib/02_InsertData.sql"
   sqlcmd -S localhost\SQLEXPRESS -E -i "lib/03_CreateSchedule.sql"
   ```

3. **Cập nhật connection string** (nếu cần) trong `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=QLQuyTinhThuong;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

4. **Chạy ứng dụng**:
   ```bash
   dotnet run
   ```

5. **Truy cập**: `https://localhost:5289`

## 🔑 Tài Khoản Mẫu

| Username | Password | Vai trò |
|----------|----------|---------|
| `admin` | `123456` | Admin |
| `manager1` | `123456` | Manager |
| `manager2` | `123456` | Manager |
| `staff1` | `123456` | Staff |
| `staff2` | `123456` | Staff |
| `staff3` | `123456` | Staff |
| `accountant1` | `123456` | Accountant (Khách hàng) |
| `accountant2` | `123456` | Accountant (Khách hàng) |

> ⚠️ Mật khẩu được hash bằng SHA256

## ✨ Tính Năng Nổi Bật

### Quản lý quyên góp
- Tạo khoản quyên góp với QR code thanh toán
- Xác nhận và theo dõi lịch sử quyên góp
- Cập nhật tự động số dư quỹ

### Quy trình hỗ trợ
1. Manager tạo công việc và phân công cho Staff
2. Staff nhận việc và bắt đầu thực hiện
3. Staff có thể yêu cầu hỗ trợ thêm nếu cần
4. Manager duyệt/từ chối yêu cầu
5. Staff hoàn thành và báo cáo
6. Khách hàng có thể xem và gửi phản hồi

### Thông báo real-time
- Thông báo khi có công việc mới
- Thông báo khi yêu cầu được duyệt/từ chối
- Thông báo khi có phản hồi từ khách hàng
- Badge hiển thị số thông báo chưa đọc

### Báo cáo thống kê
- Tổng quyên góp và chi tiêu
- Số lượng hỗ trợ đã hoàn thành
- Thống kê theo thời gian

## 📝 Ghi Chú

- Tất cả Status của User sử dụng tiếng Anh: `Active`, `Locked`, `Pending`
- Các Status khác (Task, Request, Complaint) vẫn dùng tiếng Việt
- Triggers tự động cập nhật số dư quỹ khi có quyên góp hoặc chi tiêu

## 📄 License

© 2026 - Quỹ Tình Thương. All rights reserved.
