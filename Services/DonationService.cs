using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using FoundationDonationSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;
namespace FoundationDonationSystem.Services
{
    public class DonationService : IDonationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IReferenceNumberService _referenceNumberService;
        private readonly IFileUploadService _fileUploadService;
        public DonationService(
            ApplicationDbContext context,
            IReferenceNumberService referenceNumberService,
            IFileUploadService fileUploadService)
        {
            _context = context;
            _referenceNumberService = referenceNumberService;
            _fileUploadService = fileUploadService;
        }
        // =====================================================
        // PUBLIC ONLINE DONATION
        // =====================================================
        public async Task<Donation> CreateAsync(
            Donation donation,
            IFormFile paymentProof)
        {
            if (paymentProof == null ||
                paymentProof.Length <= 0)
            {
                throw new InvalidOperationException(
                    "Payment proof is required.");
            }
            // =================================================
            // NORMALIZE TRANSACTION ID BEFORE DUPLICATE CHECK
            // =================================================
            donation.TransactionId =
                donation.TransactionId.Trim();
            // =================================================
            // DUPLICATE TRANSACTION CHECK
            //
            // Only online/public donations are checked here.
            // Offline transaction IDs are generated internally.
            // Case is ignored so ABC123 and abc123 are treated
            // as the same transaction number.
            // =================================================
            if (!string.IsNullOrWhiteSpace(
                    donation.TransactionId))
            {
                var transactionId =
                    donation.TransactionId;
                var existingDonation =
                    await _context.Donations
                        .AsNoTracking()
                        .Where(x =>
                            !x.IsOfflineDonation &&
                            x.TransactionId != null &&
                            x.TransactionId.ToUpper() ==
                                transactionId.ToUpper())
                        .Select(x => new
                        {
                            x.DonationNumber
                        })
                        .FirstOrDefaultAsync();
                if (existingDonation != null)
                {
                    throw new InvalidOperationException(
                        "DUPLICATE_TRANSACTION|" +
                        existingDonation.DonationNumber);
                }
            }
            // =================================================
            // GENERATE UNIQUE ONLINE DONATION NUMBER
            // =================================================
            donation.DonationNumber =
                await _referenceNumberService
                    .GenerateDonationNumberAsync();
            // =================================================
            // NORMALIZE DATA
            // =================================================
            donation.DonorName =
                donation.DonorName.Trim();
            donation.Mobile =
                donation.Mobile.Trim();
            donation.Email =
                string.IsNullOrWhiteSpace(donation.Email)
                    ? null
                    : donation.Email.Trim();
            donation.Address =
                string.IsNullOrWhiteSpace(donation.Address)
                    ? null
                    : donation.Address.Trim();
            // TransactionId was normalized before the duplicate check.
            // =================================================
            // INITIAL STATUS
            // =================================================
            donation.Status =
                DonationStatus.Pending;
            donation.IsOfflineDonation =
                false;
            donation.CreatedAt =
                DateTime.UtcNow;
            donation.UpdatedAt =
                null;
            donation.VerificationRemarks =
                null;
            donation.VerifiedByUserId =
                null;
            donation.VerifiedAt =
                null;
            donation.DonationSlipNumber =
                null;
            donation.SlipGeneratedAt =
                null;
            donation.OfflineVoucherPath =
                null;
            donation.OfflineVoucherOriginalName =
                null;
            // =================================================
            // SAVE ONLINE PAYMENT PROOF
            // =================================================
            var fileName =
                await _fileUploadService
                    .SavePaymentProofAsync(
                        paymentProof,
                        donation.DonationNumber);
            donation.PaymentProofPath =
                fileName;
            donation.PaymentProofOriginalName =
                Path.GetFileName(
                    paymentProof.FileName);
            // =================================================
            // DATABASE
            // =================================================
            _context.Donations.Add(donation);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Rollback uploaded payment proof
                if (!string.IsNullOrWhiteSpace(
                    donation.PaymentProofPath))
                {
                    await _fileUploadService
                        .DeletePaymentProofAsync(
                            donation.PaymentProofPath);
                }
                throw;
            }
            return donation;
        }
        // =====================================================
        // ADMIN OFFLINE CASH DONATION
        // =====================================================
        public async Task<Donation> CreateOfflineAsync(
            Donation donation,
            IFormFile offlineVoucher,
            string adminUserId)
        {
            // =================================================
            // VALIDATE VOUCHER
            // =================================================
            if (offlineVoucher == null ||
                offlineVoucher.Length <= 0)
            {
                throw new InvalidOperationException(
                    "Offline donation voucher is required.");
            }
            // =================================================
            // VALIDATE ADMIN USER
            // =================================================
            if (string.IsNullOrWhiteSpace(adminUserId))
            {
                throw new InvalidOperationException(
                    "Unable to identify the admin user.");
            }
            // =================================================
            // GENERATE UNIQUE OFFLINE DONATION NUMBER
            //
            // Example:
            // ICF-2026-O583921
            // =================================================
            donation.DonationNumber =
                await _referenceNumberService
                    .GenerateOfflineDonationNumberAsync();
            // =================================================
            // NORMALIZE DATA
            // =================================================
            donation.DonorName =
                donation.DonorName.Trim();
            donation.Mobile =
                donation.Mobile.Trim();
            donation.Email =
                string.IsNullOrWhiteSpace(donation.Email)
                    ? null
                    : donation.Email.Trim();
            donation.Address =
                string.IsNullOrWhiteSpace(donation.Address)
                    ? null
                    : donation.Address.Trim();
            donation.VerificationRemarks =
                string.IsNullOrWhiteSpace(
                    donation.VerificationRemarks)
                    ? null
                    : donation.VerificationRemarks.Trim();
            // =================================================
            // OFFLINE DONATION SETTINGS
            // =================================================
            donation.IsOfflineDonation =
                true;
            donation.PaymentMethod =
                PaymentMethod.Cash;
            // =================================================
            // GENERATE OFFLINE TRANSACTION ID
            //
            // Example:
            // OFFLINE-20260904134700123-A1B2C3D4
            // =================================================
            var offlineReference =
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 4)
                    .ToUpperInvariant();
            donation.TransactionId =
                "O" +
                DateTime.UtcNow.ToString(
                    "yyyyMMdd") +
                offlineReference;
            // =================================================
            // OFFLINE DONATION STARTS AS PENDING
            // =================================================
            // The admin who collects the cash donation is NOT
            // automatically verifying it. The donation must first
            // enter Pending status and then be verified/rejected
            // from the donation Details popup.
            // =================================================
            donation.Status =
                DonationStatus.Pending;
            // =================================================
            // VERIFICATION INFORMATION
            // =================================================
            donation.VerifiedByUserId =
                null;
            donation.VerifiedAt =
                null;
            // =================================================
            // AUDIT INFORMATION
            // =================================================
            // CreatedAt records when the offline donation was
            // entered into the system. Verification information
            // will be filled only when Verify() is performed.
            // =================================================
            donation.CreatedAt =
                DateTime.UtcNow;
            donation.UpdatedAt =
                null;
            // =================================================
            // ONLINE PAYMENT PROOF MUST BE EMPTY
            // =================================================
            donation.PaymentProofPath =
                null;
            donation.PaymentProofOriginalName =
                null;
            // =================================================
            // DONATION SLIP
            // =================================================
            donation.DonationSlipNumber =
                null;
            donation.SlipGeneratedAt =
                null;
            // =================================================
            // SAVE OFFLINE VOUCHER
            // =================================================
            var voucherPath =
                await _fileUploadService
                    .SaveOfflineVoucherAsync(
                        offlineVoucher,
                        donation.DonationNumber);
            donation.OfflineVoucherPath =
                voucherPath;
            donation.OfflineVoucherOriginalName =
                Path.GetFileName(
                    offlineVoucher.FileName);
            // =================================================
            // DATABASE
            // =================================================
            _context.Donations.Add(donation);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // =================================================
                // ROLLBACK UPLOADED VOUCHER
                // =================================================
                if (!string.IsNullOrWhiteSpace(
                    donation.OfflineVoucherPath))
                {
                    await _fileUploadService
                        .DeleteOfflineVoucherAsync(
                            donation.OfflineVoucherPath);
                }
                throw;
            }
            return donation;
        }
        // =====================================================
        // GET BY ID
        // =====================================================
        public async Task<Donation?> GetByIdAsync(int id)
        {
            return await _context.Donations
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == id);
        }
        // =====================================================
        // GET BY DONATION NUMBER
        // =====================================================
        public async Task<Donation?>
            GetByDonationNumberAsync(
                string donationNumber)
        {
            return await _context.Donations
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.DonationNumber ==
                         donationNumber);
        }
        // =====================================================
        // GET ALL
        // =====================================================
        public async Task<List<Donation>>
            GetAllAsync()
        {
            return await _context.Donations
                .AsNoTracking()
                .OrderByDescending(
                    x => x.CreatedAt)
                .ToListAsync();
        }
    }
}