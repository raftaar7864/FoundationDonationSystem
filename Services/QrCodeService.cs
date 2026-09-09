using Microsoft.AspNetCore.Hosting;
namespace FoundationDonationSystem.Services
{
    public class QrCodeService : IQrCodeService
    {
        private readonly IWebHostEnvironment _environment;
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };
        private static readonly HashSet<string> AllowedMimeTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };
        private const long MaxFileSize = 1 * 1024 * 1024;
        public QrCodeService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public async Task<string> SaveAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException(
                    "Please select a QR code image.");
            if (file.Length > MaxFileSize)
                throw new InvalidOperationException(
                    "QR code image cannot be larger than 1 MB.");
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException(
                    "Only JPG, JPEG, PNG and WEBP QR code images are allowed.");
            if (!AllowedMimeTypes.Contains(file.ContentType))
                throw new InvalidOperationException(
                    "Invalid QR code image type.");
            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "payment-qr");
            Directory.CreateDirectory(uploadsFolder);
            var fileName =
                $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(
                uploadsFolder,
                fileName);
            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None);
            await file.CopyToAsync(stream);
            return $"/uploads/payment-qr/{fileName}";
        }
        public Task DeleteAsync(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.CompletedTask;
            var fileName = Path.GetFileName(relativePath);
            if (string.IsNullOrWhiteSpace(fileName))
                return Task.CompletedTask;
            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "payment-qr");
            var filePath = Path.Combine(
                uploadsFolder,
                fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.CompletedTask;
        }
    }
}