using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerPortal.Core.Application.DTOs;

namespace CustomerPortal.Core.Application.Interfaces;

public interface IInvoiceRepository
{
	Task<DashboardSummaryDto?> GetDashboardSummaryAsync(Guid customerId, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<ActivityDto>> GetInvoiceActivityAsync(Guid customerId, int take, CancellationToken cancellationToken = default);
	Task<PagedInvoiceListDto> GetInvoicesAsync(Guid customerId, InvoiceFilterDto filter, int page, int pageSize, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<InvoiceListItemDto>> GetOpenInvoicesAsync(Guid customerId, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<InvoiceListItemDto>> GetInvoicesForPeriodAsync(Guid customerId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
	Task<InvoiceDetailDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
	Task MarkInvoicePaidOfflineAsync(Guid invoiceId, CancellationToken cancellationToken = default);
	Task<Guid> CreateInvoiceAsync(Guid customerId, CreateInvoiceDto invoice, CancellationToken cancellationToken = default);
}
