namespace CustomerPortal.Core.Application.DTOs;

public class ActivityDto
{
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal AmountChange { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
}
