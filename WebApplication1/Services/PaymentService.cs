using System.Text;
using System.Text.Json;
using QRCoder;

namespace QLDuLichRBAC_Upgrade.Services
{
    /// <summary>
    /// Service x? l� thanh to�n qua VietQR v� SePay
    /// </summary>
    public class PaymentService
    {
        private readonly HttpClient _httpClient;
        private const string SEPAY_API_URL = "https://my.sepay.vn/userapi";
        // Cấu hình SePay: Đăng ký tài khoản tại https://my.sepay.vn để lấy API Key và số tài khoản
        private const string SEPAY_API_KEY = "EYQJJ4VKTUFAFV55XH1FUWEGNT4KA7I6MRBJXDCCZZNIGLS9NM70AMP6KPRJQEYS"; // Thay bằng API Key từ SePay
        private const string SEPAY_ACCOUNT_NUMBER = "1518893947588"; // Thay bằng số tài khoản ngân hàng
        private const string SEPAY_BANK_CODE = "970437"; // MB Bank (Mã ngân hàng)

        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Tạo mã giao dịch duy nhất
        /// </summary>
        public string GenerateTransactionCode(int userId, int tourId)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"TOUR_PAY{userId}_{tourId}_{timestamp}";
        }

        /// <summary>
        /// Tạo mã giao dịch cho vé Jumparena
        /// </summary>
        public string GenerateTicketTransactionCode(int userId, int packageId)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"JUMP{userId}{packageId}{timestamp}";
        }

        /// <summary>
        /// Tạo mã vé duy nhất (QR Code)
        /// </summary>
        public string GenerateTicketCode(int userId, int packageId)
        {
            string guid = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"JPA{userId:D4}{packageId:D3}{guid}";
        }

        /// <summary>
        /// T?o chu?i VietQR theo chu?n EMVCo
        /// </summary>
        public string GenerateVietQRData(string accountNumber, string bankCode, int amount, string memo)
        {
            StringBuilder qr = new StringBuilder();

            // Payload Format Indicator
            qr.Append("000201");
            // Point of Initiation Method
            qr.Append("010212");

            // Merchant Account Info - VietQR Standard
            string gui = "A000000727";
            string shortBankCode = bankCode.Substring(0, 4); // 970437 -> 9704
            string merchantVal = $"0006{shortBankCode}2201{accountNumber.Length:D2}{accountNumber}";

            string accInfo = $"00{gui.Length:D2}{gui}" +
                           $"01{merchantVal.Length:D2}{merchantVal}" +
                           $"02{"QRIBFTTA".Length:D2}QRIBFTTA";

            qr.Append($"38{accInfo.Length:D2}{accInfo}");

            // Merchant Category Code
            qr.Append("52040000");
            // Currency Code (VND = 704)
            qr.Append("5303704");

            // Transaction Amount
            if (amount > 0)
            {
                string amtStr = amount.ToString();
                qr.Append($"54{amtStr.Length:D2}{amtStr}");
            }

            // Country Code
            qr.Append("5802VN");

            // Additional Data (memo)
            if (!string.IsNullOrEmpty(memo))
            {
                string addData = $"08{memo.Length:D2}{memo}";
                qr.Append($"62{addData.Length:D2}{addData}");
            }

            // CRC Checksum
            qr.Append("6304");
            string dataForCRC = qr.ToString();
            string crc = CalculateCRC16(dataForCRC);
            qr.Append(crc);

            return qr.ToString();
        }

        /// <summary>
        /// T�nh CRC16 cho VietQR
        /// </summary>
        private string CalculateCRC16(string data)
        {
            int crc = 0xFFFF;
            int polynomial = 0x1021;

            foreach (char c in data)
            {
                crc ^= (c << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (crc << 1) ^ polynomial;
                    }
                    else
                    {
                        crc <<= 1;
                    }
                    crc &= 0xFFFF;
                }
            }

            return crc.ToString("X4");
        }

        /// <summary>
        /// T?o QR Code t? chu?i VietQR v� tr? v? base64
        /// </summary>
        public string GenerateQRCodeBase64(string qrData, int pixelSize = 300)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }

        /// <summary>
        /// Tạo URL VietQR (dùng API VietQR.io)
        /// </summary>
        public string GenerateQRCodeUrl(decimal amount, string description)
        {
            // Sử dụng VietQR.io API để tạo QR
            string bankBin = "970422"; // MB Bank (hoặc lấy từ config)
            string accountNo = SEPAY_ACCOUNT_NUMBER;
            string template = "compact2";
            string accountName = "QUY TINH THUONG";
            
            // Làm sạch description (bỏ dấu, ký tự đặc biệt)
            string cleanDescription = System.Text.RegularExpressions.Regex.Replace(description, @"[^a-zA-Z0-9\s]", "");
            cleanDescription = cleanDescription.Replace(" ", "%20");
            
            string url = $"https://img.vietqr.io/image/{SEPAY_BANK_CODE}-{accountNo}-{template}.png?amount={(int)amount}&addInfo={cleanDescription}&accountName={Uri.EscapeDataString(accountName)}";
            return url;
        }

        /// <summary>
        /// Ki?m tra l?ch s? giao d?ch t? SePay API
        /// </summary>
        public async Task<bool> CheckTransactionHistory(string memo, int amount)
        {
            try
            {
                Console.WriteLine($"=== BẮT ĐẦU KIỂM TRA GIAO DỊCH ===");
                Console.WriteLine($"Mã GD cần tìm: {memo}");
                Console.WriteLine($"Số tiền cần tìm: {amount:N0} VNĐ");
                
                string url = $"{SEPAY_API_URL}/transactions/list?account_number={SEPAY_ACCOUNT_NUMBER}&limit=20";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {SEPAY_API_KEY}");
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"=== LOI khi lay lich su giao dich. Status: {response.StatusCode} ===");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error content: {errorContent}");
                    Console.WriteLine($"=== KET THUC KIEM TRA - KET QUA: FALSE (LOI API) ===");
                    return false;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📄 Response từ SePay: {jsonResponse}");
                
                var transactionResponse = JsonSerializer.Deserialize<SePayTransactionResponse>(jsonResponse, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (transactionResponse?.transactions != null && transactionResponse.transactions.Count > 0)
                {
                    Console.WriteLine($"=== Tim thay {transactionResponse.transactions.Count} giao dich gan day ===");
                    
                    foreach (var transaction in transactionResponse.transactions)
                    {
                        Console.WriteLine($"  - Noi dung: '{transaction.transaction_content}'");
                        Console.WriteLine($"    So tien (string): '{transaction.amount_in}'");
                        Console.WriteLine($"    So tien (parsed): {transaction.AmountInDecimal:N0} VND");
                        Console.WriteLine($"    Ngay: {transaction.transaction_date}");
                        
                        // So sánh linh hoạt: loại bỏ khoảng trắng và chuyển về chữ thường
                        string normalizedContent = transaction.transaction_content?.Trim().ToUpper() ?? "";
                        string normalizedMemo = memo?.Trim().ToUpper() ?? "";
                        
                        bool contentMatch = normalizedContent.Contains(normalizedMemo);
                        bool amountMatch = Math.Abs(transaction.AmountInDecimal - amount) < 1; // Cho phép sai số nhỏ
                        
                        Console.WriteLine($"    So sanh: Content={contentMatch}, Amount={amountMatch}");
                        Console.WriteLine($"    Transaction amount: {transaction.AmountInDecimal}, Expected: {amount}");
                        
                        if (contentMatch && amountMatch)
                        {
                            Console.WriteLine($"==> TIM THAY GIAO DICH KHOP!");
                            return true;
                        }
                    }
                    
                    Console.WriteLine($"=== KHONG tim thay giao dich khop voi ma '{memo}' va so tien {amount:N0} VND ===");
                }
                else
                {
                    Console.WriteLine($"=== KHONG co giao dich nao trong danh sach ===");
                }

                Console.WriteLine($"=== KET THUC KIEM TRA - KET QUA: FALSE ===");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== EXCEPTION khi kiem tra giao dich ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"=== KET THUC KIEM TRA - KET QUA: FALSE (EXCEPTION) ===");
                return false;
            }
        }

        /// <summary>
        /// T?o th�ng tin thanh to�n ??y ??
        /// </summary>
        public PaymentInfo CreatePaymentInfo(int userId, int tourId, decimal amount, string tourName)
        {
            string transactionCode = GenerateTransactionCode(userId, tourId);
            string qrData = GenerateVietQRData(
                SEPAY_ACCOUNT_NUMBER,
                SEPAY_BANK_CODE,
                (int)amount,
                transactionCode
            );
            string qrBase64 = GenerateQRCodeBase64(qrData);

            return new PaymentInfo
            {
                TransactionCode = transactionCode,
                Amount = amount,
                QRCodeBase64 = qrBase64,
                QRData = qrData,
                TourName = tourName,
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// T?o th�ng tin thanh to�n cho v� Jumparena
        /// </summary>
        public JumparenaPaymentInfo CreateJumparenaPaymentInfo(int userId, int packageId, decimal amount, string packageName)
        {
            string transactionCode = GenerateTicketTransactionCode(userId, packageId);
            string ticketCode = GenerateTicketCode(userId, packageId);
            string qrData = GenerateVietQRData(
                SEPAY_ACCOUNT_NUMBER,
                SEPAY_BANK_CODE,
                (int)amount,
                transactionCode
            );
            string qrBase64 = GenerateQRCodeBase64(qrData);

            return new JumparenaPaymentInfo
            {
                TransactionCode = transactionCode,
                TicketCode = ticketCode,
                Amount = amount,
                QRCodeBase64 = qrBase64,
                QRData = qrData,
                PackageName = packageName,
                CreatedAt = DateTime.Now
            };
        }
    }

    // DTO cho SePay API Response
    public class SePayTransactionResponse
    {
        public List<SePayTransaction> transactions { get; set; } = new();
    }

    public class SePayTransaction
    {
        public string transaction_content { get; set; } = string.Empty;
        public string amount_in { get; set; } = string.Empty; // SePay trả về string, không phải number
        public string transaction_date { get; set; } = string.Empty;
        public string? reference_number { get; set; }
        public string? bank_account { get; set; }
        
        // Helper property để parse amount
        public decimal AmountInDecimal 
        { 
            get 
            {
                // Dùng InvariantCulture để parse đúng format "2000.00" = 2000 VNĐ
                if (decimal.TryParse(amount_in, System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out decimal result))
                    return result;
                return 0;
            }
        }
    }

    // Model thông tin thanh toán
    public class PaymentInfo
    {
        public string TransactionCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string QRCodeBase64 { get; set; } = string.Empty;
        public string QRData { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Model thông tin thanh toán Jumparena
    public class JumparenaPaymentInfo
    {
        public string TransactionCode { get; set; } = string.Empty;
        public string TicketCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string QRCodeBase64 { get; set; } = string.Empty;
        public string QRData { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
