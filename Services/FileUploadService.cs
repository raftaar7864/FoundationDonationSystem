using Microsoft.AspNetCore.Http;
namespace FoundationDonationSystem.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private const long MaxFileSize =
            1 * 1024 * 1024; // 1 MB
        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".pdf"
        };
        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp",
            "application/pdf"
        };
        public FileUploadService(
            IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        // =========================================================
        // ONLINE PAYMENT PROOF
        // =========================================================
        public async Task<string> SavePaymentProofAsync(
            IFormFile file,
            string donationNumber)
        {
            ValidateUploadedFile(
                file,
                "Payment proof is required.");
            var uploadDirectory =
                GetPrivateUploadDirectory(
                    "DonationProofs");
            var safeFileName =
                GenerateSafeFileName(file);
            var fullPath =
                Path.Combine(
                    uploadDirectory,
                    safeFileName);
            await SaveFileAsync(
                file,
                fullPath);
            return safeFileName;
        }
        public Task DeletePaymentProofAsync(
            string? filePath)
        {
            DeletePrivateFile(
                filePath,
                "DonationProofs");
            return Task.CompletedTask;
        }
        // =========================================================
        // OFFLINE DONATION VOUCHER
        // =========================================================
        public async Task<string> SaveOfflineVoucherAsync(
            IFormFile file,
            string donationNumber)
        {
            ValidateUploadedFile(
                file,
                "Offline donation voucher is required.");
            var uploadDirectory =
                GetPrivateUploadDirectory(
                    "OfflineDonationVouchers");
            var safeFileName =
                GenerateSafeFileName(file);
            var fullPath =
                Path.Combine(
                    uploadDirectory,
                    safeFileName);
            await SaveFileAsync(
                file,
                fullPath);
            return safeFileName;
        }
        public Task DeleteOfflineVoucherAsync(
            string? filePath)
        {
            DeletePrivateFile(
                filePath,
                "OfflineDonationVouchers");
            return Task.CompletedTask;
        }
        // =========================================================
        // GET PRIVATE UPLOAD DIRECTORY
        // =========================================================
        private string GetPrivateUploadDirectory(
            string folderName)
        {
            var uploadDirectory =
                Path.Combine(
                    _environment.ContentRootPath,
                    "PrivateUploads",
                    folderName);
            Directory.CreateDirectory(
                uploadDirectory);
            return uploadDirectory;
        }
        // =========================================================
        // GENERATE SAFE FILE NAME
        // =========================================================
        private static string GenerateSafeFileName(
            IFormFile file)
        {
            var extension =
                Path.GetExtension(
                    file.FileName)
                    .ToLowerInvariant();
            return
                Guid.NewGuid()
                    .ToString("N") +
                extension;
        }
        // =========================================================
        // SAVE FILE
        // =========================================================
        private static async Task SaveFileAsync(
            IFormFile file,
            string fullPath)
        {
            await using var stream =
                new FileStream(
                    fullPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);
            await file.CopyToAsync(stream);
            await stream.FlushAsync();
        }
        // =========================================================
        // VALIDATE FILE
        // =========================================================
        private static void ValidateUploadedFile(
            IFormFile file,
            string requiredMessage)
        {
            if (file == null ||
                file.Length <= 0)
            {
                throw new InvalidOperationException(
                    requiredMessage);
            }
            // -----------------------------------------------------
            // SIZE
            // -----------------------------------------------------
            if (file.Length > MaxFileSize)
            {
                throw new InvalidOperationException(
                    "File size cannot exceed 1 MB.");
            }
            // -----------------------------------------------------
            // EXTENSION
            // -----------------------------------------------------
            var extension =
                Path.GetExtension(
                    file.FileName)
                    .ToLowerInvariant();
            if (!AllowedExtensions.Contains(
                    extension))
            {
                throw new InvalidOperationException(
                    "Only JPG, JPEG, PNG, WEBP and PDF files are allowed.");
            }
            // -----------------------------------------------------
            // CONTENT TYPE
            // -----------------------------------------------------
            var contentType =
                file.ContentType?
                    .Trim()
                    .ToLowerInvariant();
            if (!AllowedContentTypes.Contains(
                    contentType ?? string.Empty))
            {
                throw new InvalidOperationException(
                    "Invalid file type.");
            }
        }
        // =========================================================
        // DELETE PRIVATE FILE
        // =========================================================
        private void DeletePrivateFile(
            string? filePath,
            string folderName)
        {
            if (string.IsNullOrWhiteSpace(
                    filePath))
            {
                return;
            }
            // Never allow a stored path to escape
            // the intended private folder.
            var safeFileName =
                Path.GetFileName(filePath);
            if (string.IsNullOrWhiteSpace(
                    safeFileName))
            {
                return;
            }
            var uploadDirectory =
                Path.Combine(
                    _environment.ContentRootPath,
                    "PrivateUploads",
                    folderName);
            var fullPath =
                Path.Combine(
                    uploadDirectory,
                    safeFileName);
            if (System.IO.File.Exists(
                    fullPath))
            {
                System.IO.File.Delete(
                    fullPath);
            }
        }
    }
}