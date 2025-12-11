using System;

namespace CustomerPortal.Core.Application.DTOs;

public class CreateInvoiceDto
{
	public string InvoiceNumber { get; set; } = string.Empty;
	public DateTime IssuedDate { get; set; }
	public DateTime DueDate { get; set; }
	public string Status { get; set; } = "Pending";
	public decimal SubtotalAmount { get; set; }
	public decimal DiscountAmount { get; set; }
	public decimal TaxAmount { get; set; }
	public decimal TotalAmount { get; set; }
}
