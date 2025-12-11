namespace CustomerPortal.Core.Application.DTOs;

public class DashboardSummaryDto
{
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public decimal DueBalance { get; set; }
}
