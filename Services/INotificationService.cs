namespace FoundationDonationSystem.Services
{
    public interface INotificationService
    {
        Task<NewDonationNotification?> CheckNewDonationAsync(
            int latestDonationId);
        Task<NewDonationNotification?> GetLatestDonationAsync();
    }
    public class NewDonationNotification
    {
        public int DonationId { get; set; }
        public string DonationNumber { get; set; }
            = string.Empty;
        public string DonorName { get; set; }
            = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}