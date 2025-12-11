using System;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerPortal.Core.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IInvoiceRepository Invoices { get; }
    IPaymentRepository Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
