namespace CustomerPortal.Core.Domain.Shared;

public class Activity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }

    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal AmountChange { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
}
