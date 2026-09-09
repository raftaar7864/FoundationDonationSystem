using FoundationDonationSystem.Data;
using FoundationDonationSystem.Models;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace FoundationDonationSystem.Services
{
    public class DonationSlipService : IDonationSlipService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IFoundationSettingsService _foundationSettingsService;
        public DonationSlipService(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IFoundationSettingsService foundationSettingsService)
        {
            _context = context;
            _environment = environment;
            _foundationSettingsService = foundationSettingsService;
        }
        // =========================================================
        // GET BY ID
        // =========================================================
        public async Task<Models.DonationSlip?> GetByIdAsync(int id)
        {
            return await _context.DonationSlips
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        // =========================================================
        // GET BY DONATION ID
        // =========================================================
        public async Task<Models.DonationSlip?> GetByDonationIdAsync(
            int donationId)
        {
            return await _context.DonationSlips
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DonationId == donationId);
        }
        // =========================================================
        // GET ALL
        // =========================================================
        public async Task<List<Models.DonationSlip>> GetAllAsync()
        {
            return await _context.DonationSlips
                .AsNoTracking()
                .OrderByDescending(x => x.GeneratedAt)
                .ToListAsync();
        }
        // =========================================================
        // GENERATE SLIP
        // =========================================================
        public async Task<Models.DonationSlip> GenerateAsync(
            int donationId)
        {
            var donation = await _context.Donations
                .FirstOrDefaultAsync(x => x.Id == donationId);
            if (donation == null)
            {
                throw new InvalidOperationException(
                    "Donation not found.");
            }
            if (donation.Status != Models.Enums.DonationStatus.Verified)
            {
                throw new InvalidOperationException(
                    "Donation must be verified before generating a slip.");
            }
            // -----------------------------------------------------
            // Return existing slip if already generated
            // -----------------------------------------------------
            var existingSlip = await _context.DonationSlips
                .FirstOrDefaultAsync(x => x.DonationId == donationId);
            if (existingSlip != null)
            {
                return existingSlip;
            }
            // -----------------------------------------------------
            // Repair donation slip information if necessary
            // -----------------------------------------------------
            if (string.IsNullOrWhiteSpace(donation.DonationNumber))
            {
                throw new InvalidOperationException(
                    "Donation number is missing.");
            }
            // -----------------------------------------------------
            // Generate unique slip number
            // -----------------------------------------------------
            var slipNumber =
                await GenerateUniqueSlipNumberAsync();
            var generatedAt = DateTime.UtcNow;
            // -----------------------------------------------------
            // Create slip
            // -----------------------------------------------------
            var slip = new Models.DonationSlip
            {
                DonationId = donation.Id,
                SlipNumber = slipNumber,
                GeneratedAt = generatedAt
            };
            _context.DonationSlips.Add(slip);
            // -----------------------------------------------------
            // Save
            // -----------------------------------------------------
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // A concurrent request may have generated the slip
                // between our existence check and SaveChanges.
                var concurrentSlip =
                    await _context.DonationSlips
                        .FirstOrDefaultAsync(
                            x => x.DonationId == donationId);
                if (concurrentSlip != null)
                {
                    return concurrentSlip;
                }
                throw;
            }
            return slip;
        }
        // =========================================================
        // UNIQUE SLIP NUMBER
        // =========================================================
        private async Task<string> GenerateUniqueSlipNumberAsync()
        {
            var year = DateTime.Now.Year;
            const int maxAttempts = 100;
            for (int attempt = 0;
                 attempt < maxAttempts;
                 attempt++)
            {
                var randomNumber =
                    Random.Shared.Next(
                        1_000_000,
                        10_000_000);
                var slipNumber =
                    $"SLIP{randomNumber:D4}";
                var exists =
                    await _context.DonationSlips
                        .AsNoTracking()
                        .AnyAsync(
                            x => x.SlipNumber == slipNumber);
                if (!exists)
                {
                    return slipNumber;
                }
            }
            throw new InvalidOperationException(
                "Unable to generate a unique donation slip number. " +
                "Please try again.");
        }
        // =========================================================
        // GENERATE PDF
        // =========================================================
        public byte[] GeneratePdf(
            Models.Donation donation,
            Models.DonationSlip slip)
        {
            if (donation == null)
                throw new ArgumentNullException(nameof(donation));
            if (slip == null)
                throw new ArgumentNullException(nameof(slip));
            if (donation.Status != Models.Enums.DonationStatus.Verified)
            {
                throw new InvalidOperationException(
                    "Only verified donations can generate a receipt.");
            }
            QuestPDF.Settings.License = LicenseType.Community;
            // =========================================================
            // FOUNDATION SETTINGS
            // =========================================================
            var foundationSettings =
                _foundationSettingsService.GetAsync()
                    .GetAwaiter()
                    .GetResult();
            var foundationName =
                string.IsNullOrWhiteSpace(foundationSettings.FoundationName)
                    ? "FOUNDATION"
                    : foundationSettings.FoundationName.Trim();
            var foundationTagline =
                string.IsNullOrWhiteSpace(foundationSettings.Tagline)
                    ? "Serving with compassion • Supporting with purpose"
                    : foundationSettings.Tagline.Trim();
            var foundationAddress =
                string.IsNullOrWhiteSpace(foundationSettings.Address)
                    ? "-"
                    : foundationSettings.Address.Trim();
            var foundationPhone =
                string.IsNullOrWhiteSpace(foundationSettings.Phone)
                    ? "-"
                    : foundationSettings.Phone.Trim();
            var foundationEmail =
                string.IsNullOrWhiteSpace(foundationSettings.Email)
                    ? "-"
                    : foundationSettings.Email.Trim();
            var foundationWebsite =
                string.IsNullOrWhiteSpace(foundationSettings.Website)
                    ? "-"
                    : foundationSettings.Website.Trim();
            var foundationRegistration =
                string.IsNullOrWhiteSpace(foundationSettings.RegistrationNumber)
                    ? "-"
                    : foundationSettings.RegistrationNumber.Trim();
            var foundationPan =
                string.IsNullOrWhiteSpace(foundationSettings.PAN)
                    ? "-"
                    : foundationSettings.PAN.Trim();
            var receiptFooter =
                string.IsNullOrWhiteSpace(foundationSettings.ReceiptFooter)
                    ? "This is a computer-generated donation receipt. No physical signature is required."
                    : foundationSettings.ReceiptFooter.Trim();
            var thankYouMessage =
                string.IsNullOrWhiteSpace(foundationSettings.ThankYouMessage)
                    ? "Your contribution helps us continue our work and support the people and communities we serve."
                    : foundationSettings.ThankYouMessage.Trim();
            // =========================================================
            // LOGO / WATERMARK
            // =========================================================
            byte[]? logoBytes = null;
            string? resolvedLogoPath = null;
            var logoCandidates = new List<string>();
            void AddLogoCandidate(string? configuredPath)
            {
                if (string.IsNullOrWhiteSpace(configuredPath))
                    return;
                var value = configuredPath.Trim();
                if (Path.IsPathRooted(value))
                {
                    logoCandidates.Add(value);
                    return;
                }
                var relative = value
                    .TrimStart('~')
                    .TrimStart('/', '\\')
                    .Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar);
                logoCandidates.Add(
                    Path.Combine(_environment.WebRootPath, relative));
            }
            AddLogoCandidate(foundationSettings.LogoPath);
            logoCandidates.Add(Path.Combine(
                _environment.WebRootPath,
                "images",
                "foundation",
                "foundation-logo.png"));
            logoCandidates.Add(Path.Combine(
                _environment.WebRootPath,
                "images",
                "foundation-logo.png"));
            logoCandidates.Add(Path.Combine(
                _environment.WebRootPath,
                "images",
                "foundation",
                "foundation-logo.jpg"));
            logoCandidates.Add(Path.Combine(
                _environment.WebRootPath,
                "images",
                "foundation",
                "foundation-logo.jpeg"));
            logoCandidates.Add(Path.Combine(
                _environment.WebRootPath,
                "images",
                "foundation",
                "foundation-logo.webp"));
            foreach (var candidate in logoCandidates.Distinct(
                         StringComparer.OrdinalIgnoreCase))
            {
                if (!File.Exists(candidate))
                    continue;
                try
                {
                    var bytes = File.ReadAllBytes(candidate);
                    using var bitmap = SKBitmap.Decode(bytes);
                    if (bitmap != null &&
                        bitmap.Width > 0 &&
                        bitmap.Height > 0)
                    {
                        logoBytes = bytes;
                        resolvedLogoPath = candidate;
                        break;
                    }
                }
                catch
                {
                    // Try the next candidate.
                }
            }
            byte[]? watermarkBytes = null;
            if (logoBytes != null)
            {
                watermarkBytes = CreateWatermarkPng(
                    logoBytes,
                    opacity: 0.045f);
            }
            _ = resolvedLogoPath;
            // =========================================================
            // DONATION VALUES
            // =========================================================
            var donationNumber =
                string.IsNullOrWhiteSpace(donation.DonationNumber)
                    ? "-"
                    : donation.DonationNumber.Trim();
            var slipNumber =
                string.IsNullOrWhiteSpace(slip.SlipNumber)
                    ? "-"
                    : slip.SlipNumber.Trim();
            var donorName =
                string.IsNullOrWhiteSpace(donation.DonorName)
                    ? "-"
                    : donation.DonorName.Trim();
            var mobile =
                string.IsNullOrWhiteSpace(donation.Mobile)
                    ? "-"
                    : donation.Mobile.Trim();
            var email =
                string.IsNullOrWhiteSpace(donation.Email)
                    ? "-"
                    : donation.Email.Trim();
            var donorAddress =
                string.IsNullOrWhiteSpace(donation.Address)
                    ? "-"
                    : donation.Address.Trim();
            var transactionId =
                string.IsNullOrWhiteSpace(donation.TransactionId)
                    ? "-"
                    : donation.TransactionId.Trim();
            var paymentMethod =
                donation.PaymentMethod.ToString();
            var donationType =
                donation.IsOfflineDonation
                    ? "Offline Donation"
                    : "Online Donation";
            var donationDate =
                donation.TransactionDate.ToString("dd MMM yyyy");
            var generatedDate =
                slip.GeneratedAt
                    .ToLocalTime()
                    .ToString("dd MMM yyyy, hh:mm tt");
            var amount = donation.Amount;
            var amountWords = AmountInWords(amount);
            // =========================================================
            // PDF DOCUMENT
            // =========================================================
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(14);
                    page.DefaultTextStyle(style => style
                        .FontFamily("Arial")
                        .FontSize(9)
                        .FontColor("#374151"));
                    if (watermarkBytes != null)
                    {
                        page.Background()
                            .AlignCenter()
                            .AlignMiddle()
                            .Width(330)
                            .Height(330)
                            .Image(watermarkBytes)
                            .FitArea();
                    }
                    page.Content()
                        .ScaleToFit()
                        .Border(1.2f)
                        .BorderColor("#CBD5E1")
                        .Background("#FFFFFF")
                        .Column(column =>
                        {
                            column.Spacing(0);
                            // =================================================
                            // HEADER
                            // =================================================
                            column.Item()
                                .Background("#FFF7ED")
                                .BorderBottom(1.2f)
                                .BorderColor("#FDBA74")
                                .PaddingVertical(12)
                                .PaddingHorizontal(16)
                                .Row(row =>
                                {
                                    row.ConstantItem(64)
                                        .Height(64)
                                        .Background("#FFFFFF")
                                        .Border(1)
                                        .BorderColor("#FDBA74")
                                        .Padding(4)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Element(container =>
                                        {
                                            if (logoBytes != null)
                                            {
                                                container.Image(logoBytes)
                                                    .FitArea();
                                            }
                                            else
                                            {
                                                container.AlignCenter()
                                                    .AlignMiddle()
                                                    .Text("F")
                                                    .Bold()
                                                    .FontSize(26)
                                                    .FontColor("#EA580C");
                                            }
                                        });
                                    row.ConstantItem(12);
                                    row.RelativeItem()
                                        .AlignMiddle()
                                        .Column(info =>
                                        {
                                            info.Item()
                                                .Text(foundationName)
                                                .Bold()
                                                .FontSize(20)
                                                .FontColor("#9A3412");
                                            info.Item()
                                                .PaddingTop(2)
                                                .Text(foundationTagline)
                                                .FontSize(9.5f)
                                                .FontColor("#4B5563");
                                            info.Item()
                                                .PaddingTop(5)
                                                .Text(foundationAddress)
                                                .FontSize(7.8f)
                                                .FontColor("#6B7280");
                                            info.Item()
                                                .PaddingTop(2)
                                                .Text($"{foundationPhone}  •  {foundationEmail}")
                                                .FontSize(7.8f)
                                                .FontColor("#6B7280");
                                            info.Item()
                                                .PaddingTop(2)
                                                .Text($"{foundationWebsite}  •  Reg: {foundationRegistration}  •  PAN: {foundationPan}")
                                                .FontSize(7.8f)
                                                .FontColor("#6B7280");
                                        });
                                    row.ConstantItem(100)
                                        .AlignMiddle()
                                        .Column(status =>
                                        {
                                            status.Item()
                                                .AlignCenter()
                                                .Background("#9A3412")
                                                .Border(1)
                                                .CornerRadius(7)
                                                .PaddingVertical(6)
                                                .PaddingHorizontal(7)
                                                .Text("✓ VERIFIED")
                                                .Bold()
                                                .FontSize(9)
                                                .FontColor("#FFFFFF");
                                            status.Item()
                                                .PaddingTop(4)
                                                .AlignCenter()
                                                .Text("OFFICIAL RECEIPT")
                                                .Bold()
                                                .FontSize(7)
                                                .FontColor("#6B7280");
                                        });
                                });
                            // =================================================
                            // SYSTEM INFORMATION
                            // =================================================
                            column.Item()
                                .Background("#F8FAFC")
                                .BorderBottom(1)
                                .BorderColor("#E2E8F0")
                                .PaddingVertical(5)
                                .PaddingHorizontal(12)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("DONATION MANAGEMENT SYSTEM")
                                        .Bold()
                                        .FontSize(7.2f)
                                        .FontColor("#C2410C");
                                    text.Span("   •   VERIFIED DONATION RECORD   •   ")
                                        .FontSize(7.2f)
                                        .FontColor("#94A3B8");
                                    text.Span($"Generated {generatedDate}")
                                        .FontSize(7.2f)
                                        .FontColor("#64748B");
                                });
                            // =================================================
                            // TITLE
                            // =================================================
                            column.Item()
                                .PaddingTop(10)
                                .PaddingBottom(8)
                                .AlignCenter()
                                .Column(title =>
                                {
                                    title.Item()
                                        .Text("DONATION RECEIPT")
                                        .Bold()
                                        .FontSize(18)
                                        .LetterSpacing(1.2f)
                                        .FontColor("#111827");
                                    title.Item()
                                        .PaddingTop(3)
                                        .Text("OFFICIAL ACKNOWLEDGEMENT OF DONATION")
                                        .FontSize(8.2f)
                                        .AlignCenter()
                                        .LetterSpacing(0.55f)
                                        .FontColor("#64748B");
                                });
                            // =================================================
                            // REFERENCE INFORMATION
                            // =================================================
                            column.Item()
                                .PaddingHorizontal(12)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });
                                    CompactCell(table, "DONATION NO.", donationNumber, "#FFF7ED");
                                    CompactCell(table, "RECEIPT / SLIP NO.", slipNumber, "#FFF7ED");
                                    CompactCell(table, "RECEIPT GENERATED", generatedDate, "#FFF7ED");
                                });
                            // =================================================
                            // FOUNDATION INFORMATION
                            // =================================================
                            SectionTitle(column, "FOUNDATION INFORMATION");
                            column.Item()
                                .PaddingHorizontal(12)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });
                                    FoundationInfoCell(table, "REGISTERED ADDRESS", foundationAddress, 3);
                                    FoundationInfoCell(table, "PHONE", foundationPhone, 1);
                                    FoundationInfoCell(table, "EMAIL", foundationEmail, 1);
                                    FoundationInfoCell(table, "WEBSITE", foundationWebsite, 1);
                                    FoundationInfoCell(table, "REGISTRATION NUMBER", foundationRegistration, 1);
                                    FoundationInfoCell(table, "PAN", foundationPan, 1);
                                });
                            // =================================================
                            // DONOR & DONATION DETAILS
                            // =================================================
                            SectionTitle(column, "DONOR & DONATION DETAILS");
                            column.Item()
                                .PaddingHorizontal(12)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });
                                    FourColumnCell(table, "FULL NAME", donorName);
                                    FourColumnCell(table, "MOBILE NUMBER", mobile);
                                    FourColumnCell(table, "EMAIL ADDRESS", email);
                                    FourColumnCell(table, "DONATION TYPE", donationType);
                                    FourColumnCell(table, "DONATION DATE", donationDate);
                                    FourColumnCell(table, "PAYMENT METHOD", paymentMethod);
                                    FourColumnCell(table, "TRANSACTION / UTR", transactionId);
                                    FourColumnCell(table, "STATUS", "VERIFIED");
                                });
                            // =================================================
                            // TOTAL DONATION
                            // =================================================
                            column.Item()
                                .PaddingHorizontal(12)
                                .PaddingTop(9)
                                .Border(1.2f)
                                .BorderColor("#FDBA74")
                                .Background("#FFF7ED")
                                .Border(1.2f)
                                .CornerRadius(8)
                                .Padding(10)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(amountColumn =>
                                        {
                                            amountColumn.Item()
                                                .Text("TOTAL DONATION")
                                                .Bold()
                                                .FontSize(9)
                                                .LetterSpacing(0.8f)
                                                .FontColor("#C2410C");
                                            amountColumn.Item()
                                                .PaddingTop(2)
                                                .Text($"₹{amount:N2}")
                                                .Bold()
                                                .FontSize(24)
                                                .FontColor("#9A3412");
                                            amountColumn.Item()
                                                .PaddingTop(2)
                                                .Text(amountWords)
                                                .Italic()
                                                .FontSize(8.2f)
                                                .FontColor("#4B5563");
                                        });
                                    row.ConstantItem(155)
                                        .AlignMiddle()
                                        .BorderLeft(1)
                                        .BorderColor("#FDBA74")
                                        .PaddingLeft(12)
                                        .Column(note =>
                                        {
                                            note.Item()
                                                .Text("PAYMENT RECEIVED")
                                                .Bold()
                                                .FontSize(9)
                                                .FontColor("#C2410C");
                                            note.Item()
                                                .PaddingTop(3)
                                                .Text("Amount acknowledged by the foundation.")
                                                .FontSize(7.5f)
                                                .FontColor("#6B7280");
                                        });
                                });
                            // =================================================
                            // ACKNOWLEDGEMENT
                            // =================================================
                            column.Item()
                                .PaddingHorizontal(16)
                                .PaddingTop(190)
                                .PaddingBottom(9)
                                .AlignCenter()
                                .Column(thankYou =>
                                {
                                    thankYou.Item()
                                        .Text("THANK YOU FOR YOUR GENEROUS SUPPORT")
                                        .Bold()
                                        .FontSize(10)
                                        .LetterSpacing(0.4f)
                                        .FontColor("#C2410C");
                                    thankYou.Item()
                                        .PaddingTop(3)
                                        .Text(thankYouMessage)
                                        .FontSize(8)
                                        .FontColor("#64748B")
                                        .AlignCenter();
                                });
                            // =================================================
                            // SIGNATURE / SEAL
                            // =================================================
                            column.Item()
                                .PaddingHorizontal(12)
                                .PaddingTop(215)
                                .PaddingBottom(2)
                                .BorderTop(0)
                                .BorderColor("#E2E8F0")
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .AlignMiddle()
                                        .Column(signature =>
                                        {
                                            signature.Item().Height(20);
                                            signature.Item()
                                                .BorderTop(1)
                                                .BorderColor("#374151");
                                            signature.Item()
                                                .PaddingTop(3)
                                                .AlignCenter()
                                                .Text("AUTHORIZED SIGNATORY")
                                                .Bold()
                                                .FontSize(8)
                                                .FontColor("#111827");
                                            signature.Item()
                                                .PaddingTop(2)
                                                .AlignCenter()
                                                .Text($"For and on behalf of {foundationName}")
                                                .FontSize(7)
                                                .FontColor("#64748B");
                                        });
                                    row.ConstantItem(75);
                                    row.ConstantItem(68)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Column(seal =>
                                        {
                                            seal.Item()
                                                .Width(50)
                                                .Height(50)
                                                .AlignCenter()
                                                .AlignMiddle()
                                                .Border(0)
                                                .BorderColor("#D97706")
                                                .CornerRadius(0)
                                                .Column(circle =>
                                                {
                                                    circle.Item()
                                                        .AlignCenter()
                                                        .PaddingTop(6)
                                                        .Text("OFFICIAL")
                                                        .Bold()
                                                        .FontSize(5.8f)
                                                        .FontColor("#6B7280");
                                                    circle.Item()
                                                        .AlignCenter()
                                                        .PaddingTop(1)
                                                        .Text("SEAL")
                                                        .Bold()
                                                        .FontSize(8)
                                                        .FontColor("#9A3412");
                                                    circle.Item()
                                                        .AlignCenter()
                                                        .PaddingHorizontal(5)
                                                        .PaddingTop(2)
                                                        .Text(foundationName)                                                      
                                                        .FontSize(4.7f)
                                                        .FontColor("#6B7280");
                                                });
                                        });
                                });
                            // =================================================
                            // FOOTER
                            // =================================================
                            column.Item()
                                .Background("#FFF7ED")
                                .BorderTop(5)
                                .BorderColor("#FED7AA")
                                .PaddingVertical(6)
                                .PaddingHorizontal(12)
                                .AlignCenter()
                                .Column(footer =>
                                {
                                    footer.Item()
                                        .Text(receiptFooter)
                                        .FontSize(7)
                                        .FontColor("#64748B")
                                        .AlignCenter();
                                    footer.Item()
                                        .PaddingTop(7)
                                        .Text($"Donation No: {donationNumber}   •   Receipt No: {slipNumber}   •   Please retain this receipt for your records.")
                                        .FontSize(7)
                                        .FontColor("#64748B")
                                        .AlignCenter();
                                });
                        });
                });
            })
            .GeneratePdf();
        }
        private static byte[] CreateWatermarkPng(
            byte[] sourceBytes,
            float opacity)
        {
            opacity = Math.Clamp(opacity, 0f, 1f);
            using var source = SKBitmap.Decode(sourceBytes);
            if (source == null)
            {
                throw new InvalidOperationException(
                    "The foundation logo could not be decoded as an image.");
            }
            var info = new SKImageInfo(
                source.Width,
                source.Height,
                SKColorType.Rgba8888,
                SKAlphaType.Premul);
            using var bitmap = new SKBitmap(info);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Transparent);
                using var paint = new SKPaint
                {
                    IsAntialias = true,
                    Color = new SKColor(
                        255,
                        255,
                        255,
                        (byte)Math.Round(opacity * 255)),
                    BlendMode = SKBlendMode.Modulate
                };
                canvas.DrawBitmap(source, 0, 0, paint);
                canvas.Flush();
            }
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        // =========================================================
        // COMPACT RECEIPT HELPERS
        // =========================================================
        private static void CompactCell(
            TableDescriptor table,
            string label,
            string value,
            string background)
        {
            table.Cell()
                .Border(1)
                .BorderColor("#E5E7EB")
                .Background(background)
                .Padding(7)
                .Column(column =>
                {
                    column.Item()
                        .Text(label)
                        .Bold()
                        .FontSize(7.1f)
                        .FontColor("#6B7280");
                    column.Item()
                        .PaddingTop(1)
                        .Text(value)
                        .Bold()
                        .FontSize(9.2f)
                        .FontColor("#111827");
                });
        }
        private static void FourColumnCell(
            TableDescriptor table,
            string label,
            string value)
        {
            table.Cell()
                .Border(1)
                .BorderColor("#E5E7EB")
                .Padding(7)
                .Column(column =>
                {
                    column.Item()
                        .Text(label)
                        .Bold()
                        .FontSize(7f)
                        .FontColor("#6B7280");
                    column.Item()
                        .PaddingTop(1)
                        .Text(value)
                        .FontSize(8.8f)
                        .FontColor("#374151");
                });
        }
        private static void SectionTitle(
            ColumnDescriptor column,
            string title)
        {
            column.Item()
                .PaddingTop(9)
                .PaddingBottom(5)
                .PaddingHorizontal(12)
                .Row(row =>
                {
                    row.ConstantItem(4)
                        .Height(18)
                        .Background("#EA580C");
                    row.ConstantItem(6);
                    row.RelativeItem()
                        .AlignMiddle()
                        .Text(title)
                        .Bold()
                        .FontSize(8.8f)
                        .LetterSpacing(0.35f)
                        .FontColor("#C2410C");
                });
        }
        // =========================================================
        // REFERENCE CELL
        // =========================================================
        private static void ReferenceCell(
            TableDescriptor table,
            string label,
            string value,
            bool first)
        {
            table.Cell()
                .Border(1)
                .BorderColor("#D1D5DB")
                .Background("#F9FAFB")
                .Padding(8)
                .Column(column =>
                {
                    column.Item()
                        .Text(label)
                        .Bold()
                        .FontSize(6.8f)
                        .FontColor("#6B7280");
                    column.Item()
                        .PaddingTop(2)
                        .Text(value)
                        .Bold()
                        .FontSize(9)
                        .FontColor("#111827");
                });
        }
        // =========================================================
        // DETAIL CELL
        // =========================================================
        private static void DetailCell(
            TableDescriptor table,
            string label,
            string value,
            bool left)
        {
            table.Cell()
                .Border(1)
                .BorderColor("#E5E7EB")
                .Padding(8)
                .Column(column =>
                {
                    column.Item()
                        .Text(label)
                        .Bold()
                        .FontSize(6.8f)
                        .FontColor("#6B7280");
                    column.Item()
                        .PaddingTop(2)
                        .Text(value)
                        .FontSize(9)
                        .FontColor("#374151");
                });
        }
        // =========================================================
        // FOUNDATION INFORMATION CELL
        // =========================================================
        private static void FoundationInfoCell(
            TableDescriptor table,
            string label,
            string value,
            int span)
        {
            if (span > 1)
            {
                table.Cell()
                    .ColumnSpan((uint)span)
                    .Border(1)
                    .BorderColor("#E5E7EB")
                    .Background("#FFFFFF")
                    .Padding(7)
                    .Column(column =>
                    {
                        column.Item()
                            .Text(label)
                            .Bold()
                            .FontSize(7.5f)
                            .FontColor("#6B7280");
                        column.Item()
                            .PaddingTop(1.5f)
                            .Text(value)
                            .FontSize(9f)
                            .FontColor("#374151");
                    });
                return;
            }
            table.Cell()
                .Border(1)
                .BorderColor("#E5E7EB")
                .Background("#FFFFFF")
                .Padding(6)
                .Column(column =>
                {
                    column.Item()
                        .Text(label)
                        .Bold()
                        .FontSize(7.2f)
                        .FontColor("#6B7280");
                    column.Item()
                        .PaddingTop(1.5f)
                        .Text(value)
                        .FontSize(8.2f)
                        .FontColor("#374151");
                });
        }
        // =========================================================
        // AMOUNT IN WORDS
        // =========================================================
        private static string AmountInWords(
            decimal amount)
        {
            if (amount == 0)
            {
                return "Zero Rupees Only";
            }
            long rupees =
                (long)Math.Floor(amount);
            int paise =
                (int)Math.Round(
                    (amount - rupees) * 100);
            string NumberToWords(long number)
            {
                if (number == 0)
                {
                    return "Zero";
                }
                if (number < 0)
                {
                    return "Minus " +
                           NumberToWords(
                               Math.Abs(number));
                }
                string[] ones =
                {
                    "",
                    "One",
                    "Two",
                    "Three",
                    "Four",
                    "Five",
                    "Six",
                    "Seven",
                    "Eight",
                    "Nine",
                    "Ten",
                    "Eleven",
                    "Twelve",
                    "Thirteen",
                    "Fourteen",
                    "Fifteen",
                    "Sixteen",
                    "Seventeen",
                    "Eighteen",
                    "Nineteen"
                };
                string[] tens =
                {
                    "",
                    "",
                    "Twenty",
                    "Thirty",
                    "Forty",
                    "Fifty",
                    "Sixty",
                    "Seventy",
                    "Eighty",
                    "Ninety"
                };
                if (number < 20)
                {
                    return ones[number];
                }
                if (number < 100)
                {
                    return tens[number / 10] +
                           (number % 10 > 0
                               ? " " +
                                 ones[number % 10]
                               : "");
                }
                if (number < 1000)
                {
                    return ones[number / 100] +
                           " Hundred" +
                           (number % 100 > 0
                               ? " " +
                                 NumberToWords(
                                     number % 100)
                               : "");
                }
                if (number < 100000)
                {
                    return NumberToWords(
                               number / 1000) +
                           " Thousand" +
                           (number % 1000 > 0
                               ? " " +
                                 NumberToWords(
                                     number % 1000)
                               : "");
                }
                if (number < 10000000)
                {
                    return NumberToWords(
                               number / 100000) +
                           " Lakh" +
                           (number % 100000 > 0
                               ? " " +
                                 NumberToWords(
                                     number % 100000)
                               : "");
                }
                return NumberToWords(
                           number / 10000000) +
                       " Crore" +
                       (number % 10000000 > 0
                           ? " " +
                             NumberToWords(
                                 number % 10000000)
                           : "");
            }
            var result =
                NumberToWords(rupees) +
                " Rupees";
            if (paise > 0)
            {
                result +=
                    " and " +
                    NumberToWords(paise) +
                    " Paise";
            }
            return result + " Only";
        }
    }
}