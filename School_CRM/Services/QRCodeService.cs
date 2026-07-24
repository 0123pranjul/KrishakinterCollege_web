using QRCoder;
using System.Text.Json;
using School_CRM.Models.DTOs;

namespace School_CRM.Services
{
    public class QRCodeService
    {
        private readonly string _qrCodeDirectory;

        public QRCodeService(IWebHostEnvironment env)
        {
            _qrCodeDirectory = Path.Combine(env.WebRootPath, "qrcodes");
            if (!Directory.Exists(_qrCodeDirectory))
                Directory.CreateDirectory(_qrCodeDirectory);
        }

        /// <summary>
        /// Generate QR Code image and save to disk
        /// </summary>
        public string GenerateQRCode(QRCodeDataDto data)
        {
            // Serialize data to JSON
            var jsonData = JsonSerializer.Serialize(data);

            // Generate QR Code
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(jsonData, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
            byte[] qrBytes = qrCode.GetGraphic(20);

            // Save to file
            var fileName = $"{data.AccessionNo}.png";
            var filePath = Path.Combine(_qrCodeDirectory, fileName);
            File.WriteAllBytes(filePath, qrBytes);

            return $"/qrcodes/{fileName}";
        }

        /// <summary>
        /// Get QR Code image bytes
        /// </summary>
        public byte[]? GetQRCodeImage(string accessionNo)
        {
            var fileName = $"{accessionNo}.png";
            var filePath = Path.Combine(_qrCodeDirectory, fileName);

            if (!File.Exists(filePath))
                return null;

            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// Delete QR Code image
        /// </summary>
        public bool DeleteQRCode(string accessionNo)
        {
            var fileName = $"{accessionNo}.png";
            var filePath = Path.Combine(_qrCodeDirectory, fileName);

            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }

        /// <summary>
        /// Generate QR Code as Base64 string (for inline display)
        /// </summary>
        public string GenerateQRCodeBase64(QRCodeDataDto data)
        {
            var jsonData = JsonSerializer.Serialize(data);

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(jsonData, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
            byte[] qrBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrBytes);
        }
    }
}
