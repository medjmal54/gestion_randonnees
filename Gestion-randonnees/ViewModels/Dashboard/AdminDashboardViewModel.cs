namespace Gestion_randonnees.ViewModels.Dashboard;

public class AdminDashboardViewModel
{
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int TotalHikes { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRequests { get; set; }
    public int AcceptedRequests { get; set; }
    public int RefusedRequests { get; set; }
    public int PendingRequests { get; set; }
    public decimal Revenue { get; set; }
}
