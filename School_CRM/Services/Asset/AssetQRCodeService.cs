using QRCoder;
using System.Text.Json;
using School_CRM.Models.DTOs;

namespace School_CRM.Services.Asset
{
    public class AssetQRCodeService
    {
        private readonly string _qrDir;

        public AssetQRCodeService(IWebHostEnvironment env)
        {
            _qrDir = Path.Combine(env.WebRootPath, "assetqr");
            Directory.CreateDirectory(_qrDir);
        }

        public string GenerateQRCode(AssetQRDataDto data)
        {
            var json = JsonSerializer.Serialize(data);

            using var gen  = new QRCodeGenerator();
            using var qrData = gen.CreateQrCode(json, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);

            var bytes    = qrCode.GetGraphic(20);
            var fileName = $"{data.AssetTag}.png";
            File.WriteAllBytes(Path.Combine(_qrDir, fileName), bytes);

            return $"/assetqr/{fileName}";
        }

        public byte[]? GetQRImage(string assetTag)
        {
            var path = Path.Combine(_qrDir, $"{assetTag}.png");
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        public string GenerateBase64(AssetQRDataDto data)
        {
            var json = JsonSerializer.Serialize(data);
            using var gen    = new QRCodeGenerator();
            using var qrData = gen.CreateQrCode(json, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            return Convert.ToBase64String(qrCode.GetGraphic(20));
        }
    }
}
