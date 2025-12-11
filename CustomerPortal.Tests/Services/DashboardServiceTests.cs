using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerPortal.Core.Application.DTOs;
using CustomerPortal.Core.Application.Interfaces;
using CustomerPortal.Web.Services;
using Xunit;

namespace CustomerPortal.Tests.Services;

public class DashboardServiceTests
{
    private sealed class FakeInvoiceRepository : IInvoiceRepository
    {
        public DashboardSummaryDto? Summary { get; set; }
        public IReadOnlyList<ActivityDto> InvoiceActivity { get; set; } = Array.Empty<ActivityDto>();

        public Task<DashboardSummaryDto?> GetDashboardSummaryAsync(Guid customerId, CancellationToken cancellationToken = default)
            => Task.FromResult(Summary);

        public Task<IReadOnlyList<ActivityDto>> GetInvoiceActivityAsync(Guid customerId, int take, CancellationToken cancellationToken = default)
            => Task.FromResult(InvoiceActivity);

		public Task<PagedInvoiceListDto> GetInvoicesAsync(Guid customerId, InvoiceFilterDto filter, int page, int pageSize, CancellationToken cancellationToken = default)
			=> Task.FromResult(new PagedInvoiceListDto());

        public Task<IReadOnlyList<InvoiceListItemDto>> GetOpenInvoicesAsync(Guid customerId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<InvoiceListItemDto>>(Array.Empty<InvoiceListItemDto>());

        public Task<IReadOnlyList<InvoiceListItemDto>> GetInvoicesForPeriodAsync(Guid customerId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<InvoiceListItemDto>>(Array.Empty<InvoiceListItemDto>());

        public Task<InvoiceDetailDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
            => Task.FromResult<InvoiceDetailDto?>(null);

        public Task MarkInvoicePaidOfflineAsync(Guid invoiceId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<Guid> CreateInvoiceAsync(Guid customerId, CreateInvoiceDto invoice, CancellationToken cancellationToken = default)
            => Task.FromResult(Guid.NewGuid());
    }

    private sealed class FakePaymentRepository : IPaymentRepository
    {
        public IReadOnlyList<ActivityDto> PaymentActivity { get; set; } = Array.Empty<ActivityDto>();

        public Task<IReadOnlyList<ActivityDto>> GetPaymentActivityAsync(Guid customerId, int take, CancellationToken cancellationToken = default)
            => Task.FromResult(PaymentActivity);

		public Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(Guid customerId, DateTime? from, DateTime? to, string? method, CancellationToken cancellationToken = default)
			=> Task.FromResult<IReadOnlyList<PaymentDto>>(Array.Empty<PaymentDto>());

		public Task<Guid> CreatePaymentAsync(Guid customerId, CreatePaymentDto payment, CancellationToken cancellationToken = default)
			=> Task.FromResult(Guid.NewGuid());
    }

    [Fact]
    public async Task GetSummaryAsync_Returns_Summary_From_InvoiceRepository()
    {
        var expected = new DashboardSummaryDto
        {
            CreditLimit = 20000m,
            AvailableCredit = 15000m,
            DueBalance = 5000m
        };

        var invoiceRepo = new FakeInvoiceRepository { Summary = expected };
        var paymentRepo = new FakePaymentRepository();
        var service = new DashboardService(invoiceRepo, paymentRepo);

        var result = await service.GetSummaryAsync(Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Equal(expected.CreditLimit, result!.CreditLimit);
        Assert.Equal(expected.AvailableCredit, result.AvailableCredit);
        Assert.Equal(expected.DueBalance, result.DueBalance);
    }

    [Fact]
    public async Task GetRecentActivityAsync_Merges_And_Orders_By_Date_Descending()
    {
        var baseDate = new DateTime(2023, 9, 1);

        var invoiceActivity = new[]
        {
            new ActivityDto { Date = baseDate.AddDays(-2), Title = "Invoice A", AmountChange = 100m, Status = "due", ActivityType = "Invoice" },
            new ActivityDto { Date = baseDate.AddDays(-5), Title = "Invoice B", AmountChange = 200m, Status = "due", ActivityType = "Invoice" }
        };

        var paymentActivity = new[]
        {
            new ActivityDto { Date = baseDate.AddDays(-1), Title = "Payment A", AmountChange = -50m, Status = "paid", ActivityType = "Payment" }
        };

        var invoiceRepo = new FakeInvoiceRepository { InvoiceActivity = invoiceActivity };
        var paymentRepo = new FakePaymentRepository { PaymentActivity = paymentActivity };
        var service = new DashboardService(invoiceRepo, paymentRepo);

        var result = await service.GetRecentActivityAsync(Guid.NewGuid(), 10);

        Assert.Equal(3, result.Count);
        Assert.Equal("Payment A", result[0].Title);
        Assert.Equal("Invoice A", result[1].Title);
        Assert.Equal("Invoice B", result[2].Title);
    }
}
