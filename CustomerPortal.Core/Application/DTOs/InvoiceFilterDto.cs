namespace CustomerPortal.Core.Application.DTOs;

public class InvoiceFilterDto
{
	public string? InvoiceNumber { get; set; }
	public string? Status { get; set; }
	public decimal? MinAmount { get; set; }
	public decimal? MaxAmount { get; set; }
}
