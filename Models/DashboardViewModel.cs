namespace GauGhar.Models
{
    public class DashboardViewModel
    {
        public int TotalCows { get; set; }
        public int ActiveCows { get; set; }
        public int PregnantCows { get; set; }
        public decimal TodayMilk { get; set; }
        public decimal TodaySales { get; set; }
        public decimal TodayExpenses { get; set; }
    }
}
