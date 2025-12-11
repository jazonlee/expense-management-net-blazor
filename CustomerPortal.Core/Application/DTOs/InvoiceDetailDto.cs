namespace CustomerPortal.Core.Application.DTOs;

public class InvoiceDetailDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public string BillToName { get; set; } = string.Empty;
    public string BillToAddress { get; set; } = string.Empty;
    public string ShipToName { get; set; } = string.Empty;
    public string ShipToAddress { get; set; } = string.Empty;

    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public IReadOnlyList<InvoiceDetailLineItemDto> LineItems { get; set; } = Array.Empty<InvoiceDetailLineItemDto>();
}

public class InvoiceDetailLineItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
