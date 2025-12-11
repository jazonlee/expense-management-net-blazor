using System.Threading;
using System.Threading.Tasks;
using CustomerPortal.Core.Application.Interfaces;

namespace CustomerPortal.Web.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    public IInvoiceRepository Invoices { get; }
    public IPaymentRepository Payments { get; }

    public UnitOfWork(IInvoiceRepository invoiceRepository, IPaymentRepository paymentRepository)
    {
        Invoices = invoiceRepository;
        Payments = paymentRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Transactional behavior will be implemented in Phase 2 (Foundational tasks).
        return Task.FromResult(0);
    }

    public void Dispose()
    {
        // Nothing to dispose yet; connection/transactions will be managed in Phase 2.
    }
}
