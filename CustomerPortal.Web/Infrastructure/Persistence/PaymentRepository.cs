using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CustomerPortal.Core.Application.DTOs;
using CustomerPortal.Core.Application.Interfaces;
using Dapper;

namespace CustomerPortal.Web.Infrastructure.Persistence;

public class PaymentRepository : IPaymentRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public PaymentRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ActivityDto>> GetPaymentActivityAsync(Guid customerId, int take, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT TOP (@Take)
            p.[Date] AS [Date],
            CONCAT('Payment ', p.PaymentRef) AS Title,
            -p.Amount AS AmountChange,
            'Paid' AS Status,
            'Payment' AS ActivityType
        FROM Payments p
        WHERE p.CustomerId = @CustomerId
        ORDER BY p.[Date] DESC;";

        using var connection = _connectionFactory.CreateConnection();
        var effectiveCustomerId = await ResolveCustomerIdAsync(customerId, connection, cancellationToken);
        var command = new CommandDefinition(sql, new { CustomerId = effectiveCustomerId, Take = take }, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<ActivityDto>(command);
        return results.AsList();
    }

    public async Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(Guid customerId, DateTime? from, DateTime? to, string? method, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT
            Id,
            [Date],
            Amount,
            [Method] AS Method,
            PaymentRef
        FROM Payments
        WHERE CustomerId = @CustomerId
            AND (@From IS NULL OR [Date] >= @From)
            AND (@To IS NULL OR [Date] <= @To)
            AND (@Method IS NULL OR @Method = '' OR [Method] = @Method)
        ORDER BY [Date] DESC;";

        using var connection = _connectionFactory.CreateConnection();
        var effectiveCustomerId = await ResolveCustomerIdAsync(customerId, connection, cancellationToken);
        var parameters = new
        {
            CustomerId = effectiveCustomerId,
            From = from,
            To = to,
            Method = string.IsNullOrWhiteSpace(method) ? null : method
        };

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<PaymentDto>(command);
        return results.AsList();
    }

    public async Task<Guid> CreatePaymentAsync(Guid customerId, CreatePaymentDto payment, CancellationToken cancellationToken = default)
    {
        const string sql = @"INSERT INTO Payments (
            Id,
            CustomerId,
            PaymentRef,
            [Date],
            Amount,
            [Method],
            ReceiptUrl
        ) VALUES (
            @Id,
            @CustomerId,
            @PaymentRef,
            @Date,
            @Amount,
            @Method,
            @ReceiptUrl
        );";

        var id = Guid.NewGuid();
        using var connection = _connectionFactory.CreateConnection();
        var effectiveCustomerId = await ResolveCustomerIdAsync(customerId, connection, cancellationToken);
        var parameters = new
        {
            Id = id,
            CustomerId = effectiveCustomerId,
            payment.PaymentRef,
            payment.Date,
            payment.Amount,
            payment.Method,
            ReceiptUrl = (string?)null
        };

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
        return id;
    }

    private static async Task<Guid> ResolveCustomerIdAsync(Guid customerId, IDbConnection connection, CancellationToken cancellationToken)
    {
        if (customerId != Guid.Empty)
        {
            return customerId;
        }

        const string sql = "SELECT TOP (1) CustomerId FROM Customers ORDER BY CustomerNumber;";
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var existingId = await connection.QueryFirstOrDefaultAsync<Guid?>(command);
        if (existingId is null || existingId == Guid.Empty)
        {
            throw new InvalidOperationException("No customers are available in the database to associate with payments.");
        }

        return existingId.Value;
    }
}
