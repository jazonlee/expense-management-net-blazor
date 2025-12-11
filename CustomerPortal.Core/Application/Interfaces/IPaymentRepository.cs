using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerPortal.Core.Application.DTOs;

namespace CustomerPortal.Core.Application.Interfaces;

public interface IPaymentRepository
{
	Task<IReadOnlyList<ActivityDto>> GetPaymentActivityAsync(Guid customerId, int take, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(Guid customerId, DateTime? from, DateTime? to, string? method, CancellationToken cancellationToken = default);
	Task<Guid> CreatePaymentAsync(Guid customerId, CreatePaymentDto payment, CancellationToken cancellationToken = default);
}
