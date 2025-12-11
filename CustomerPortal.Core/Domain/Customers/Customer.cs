namespace CustomerPortal.Core.Domain.Customers;

public class Customer
{
    public Guid CustomerId { get; set; }
    public string CustomerNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string BillingAddressLine1 { get; set; } = string.Empty;
    public string? BillingAddressLine2 { get; set; }
    public string BillingCity { get; set; } = string.Empty;
    public string? BillingState { get; set; }
    public string BillingPostalCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;

    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string? ShippingState { get; set; }
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;

    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit { get; set; }
    public decimal CurrentBalance { get; set; }
}
