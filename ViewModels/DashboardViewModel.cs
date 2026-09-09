using FoundationDonationSystem.Models;
namespace FoundationDonationSystem.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TodayDonationAmount { get; set; }
        public decimal MonthDonationAmount { get; set; }
        public int VerifiedDonationCount { get; set; }
        public int PendingDonationCount { get; set; }
        public int TotalDonationCount { get; set; }
        public int LatestDonationId { get; set; }
        public string? LatestDonationNumber { get; set; }
        public string? LatestDonorName { get; set; }
        public decimal LatestDonationAmount { get; set; }
        public DateTime? LatestDonationDate { get; set; }
        public List<Donation> RecentDonations { get; set; }
            = new();
    }
}