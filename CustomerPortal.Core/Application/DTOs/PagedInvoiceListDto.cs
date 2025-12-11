using System;
using System.Collections.Generic;

namespace CustomerPortal.Core.Application.DTOs;

public class PagedInvoiceListDto
{
	public IReadOnlyList<InvoiceListItemDto> Items { get; set; } = Array.Empty<InvoiceListItemDto>();
	public int Page { get; set; }
	public int PageSize { get; set; }
	public int TotalCount { get; set; }
	public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
