using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerPortal.Core.Application.DTOs;
using CustomerPortal.Core.Application.Interfaces;

namespace CustomerPortal.Web.Services;

public class InvoiceService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvoiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<PagedInvoiceListDto> GetInvoicesAsync(Guid customerId, InvoiceFilterDto filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.Invoices.GetInvoicesAsync(customerId, filter, page, pageSize, cancellationToken);
    }

	public Task<IReadOnlyList<InvoiceListItemDto>> GetOpenInvoicesAsync(Guid customerId, CancellationToken cancellationToken = default)
	{
		return _unitOfWork.Invoices.GetOpenInvoicesAsync(customerId, cancellationToken);
	}

    public Task<InvoiceDetailDto?> GetInvoiceDetailAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.Invoices.GetInvoiceByIdAsync(invoiceId, cancellationToken);
    }

    public async Task MarkInvoicePaidOfflineAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Invoices.MarkInvoicePaidOfflineAsync(invoiceId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> CreateInvoiceAsync(Guid customerId, CreateInvoiceDto invoice, CancellationToken cancellationToken = default)
    {
        var id = await _unitOfWork.Invoices.CreateInvoiceAsync(customerId, invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return id;
    }
}
