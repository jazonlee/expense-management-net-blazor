using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerPortal.Core.Application.DTOs;
using CustomerPortal.Core.Application.Interfaces;

namespace CustomerPortal.Web.Services;

public class DashboardService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public DashboardService(IInvoiceRepository invoiceRepository, IPaymentRepository paymentRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public Task<DashboardSummaryDto?> GetSummaryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return _invoiceRepository.GetDashboardSummaryAsync(customerId, cancellationToken);
    }

    public async Task<IReadOnlyList<ActivityDto>> GetRecentActivityAsync(Guid customerId, int take = 10, CancellationToken cancellationToken = default)
    {
        var invoiceActivity = await _invoiceRepository.GetInvoiceActivityAsync(customerId, take, cancellationToken);
        var paymentActivity = await _paymentRepository.GetPaymentActivityAsync(customerId, take, cancellationToken);

        var combined = invoiceActivity
            .Concat(paymentActivity)
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.AmountChange)
            .Take(take)
            .ToArray();

        return combined;
    }
}
