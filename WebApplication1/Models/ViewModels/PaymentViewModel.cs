namespace QLDuLichRBAC_Upgrade.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho thanh to�n
    /// </summary>
    public class PaymentViewModel
    {
        public int MaTour { get; set; }
        public string TenTour { get; set; } = string.Empty;
        public int SoLuongVe { get; set; }
        public decimal GiaTour { get; set; }
        public List<DichVuViewModel> DichVuDaChon { get; set; } = new();
        public decimal TongTien { get; set; }
        public string QRCodeBase64 { get; set; } = string.Empty;
        public string MaGiaoDich { get; set; } = string.Empty;
    }

    public class DichVuViewModel
    {
        public int MaDV { get; set; }
        public string TenDV { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
    }

    /// <summary>
    /// Model cho phản hồi kiểm tra thanh toán
    /// </summary>
    public class PaymentCheckResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel cho thanh toán vé Jumparena
    /// </summary>
    public class JumparenaPaymentViewModel
    {
        public int MaGoi { get; set; }
        public string TenGoi { get; set; } = string.Empty;
        public int MaCa { get; set; }
        public string TenCa { get; set; } = string.Empty;
        public DateTime NgayDat { get; set; }
        public DateTime NgaySuDung { get; set; }
        public int SoNguoi { get; set; }
        public decimal GiaGoi { get; set; }
        public List<DichVuThemViewModel> DichVuDaChon { get; set; } = new();
        public decimal TongTien { get; set; }
        public string QRCodeBase64 { get; set; } = string.Empty;
        public string MaGiaoDich { get; set; } = string.Empty;
        public string MaVeCode { get; set; } = string.Empty;
    }

    public class DichVuThemViewModel
    {
        public int MaDVThem { get; set; }
        public string TenDV { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; } = 1;
    }

    /// <summary>
    /// ViewModel cho vé đã đặt
    /// </summary>
    public class MyTicketViewModel
    {
        public int MaVe { get; set; }
        public string MaVeCode { get; set; } = string.Empty;
        public string TenGoi { get; set; } = string.Empty;
        public string TenCa { get; set; } = string.Empty;
        public DateTime NgayDat { get; set; }
        public DateTime NgaySuDung { get; set; }
        public int SoNguoi { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public DateTime? NgayCheckIn { get; set; }
        public List<DichVuThemViewModel> DichVuThem { get; set; } = new();
        public string QRCodeBase64 { get; set; } = string.Empty;
    }
}
