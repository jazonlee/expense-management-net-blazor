using System;

namespace CustomerPortal.Core.Application.DTOs;

public class CreatePaymentDto
{
	public DateTime Date { get; set; }
	public decimal Amount { get; set; }
	public string Method { get; set; } = string.Empty;
	public string PaymentRef { get; set; } = string.Empty;
}
