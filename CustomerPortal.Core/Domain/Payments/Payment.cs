namespace CustomerPortal.Core.Domain.Payments;

public class Payment
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }

    public string PaymentRef { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? ReceiptUrl { get; set; }
}
