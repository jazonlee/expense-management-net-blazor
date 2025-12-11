using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CustomerPortal.Core.Application.DTOs;
using CustomerPortal.Core.Application.Interfaces;
using Dapper;

namespace CustomerPortal.Web.Infrastructure.Persistence;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public InvoiceRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DashboardSummaryDto?> GetDashboardSummaryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT
            c.CreditLimit,
            (c.CreditLimit - (ISNULL(inv.TotalOpen, 0) - ISNULL(pay.TotalPaid, 0))) AS AvailableCredit,
            (ISNULL(inv.TotalOpen, 0) - ISNULL(pay.TotalPaid, 0)) AS DueBalance
        FROM Customers c
        OUTER APPLY (
            SELECT SUM(CASE WHEN i.Status IN ('Pending', 'Overdue') THEN i.TotalAmount ELSE 0 END) AS TotalOpen
            FROM Invoices i
            WHERE i.CustomerId = c.CustomerId
        ) inv
        OUTER APPLY (
            SELECT SUM(p.Amount) AS TotalPaid
            FROM Payments p
            WHERE p.CustomerId = c.CustomerId
        ) pay
        WHERE c.CustomerId = @CustomerId;";

        using var connection = _connectionFactory.CreateConnection();
		var effectiveCustomerId = await ResolveCustomerIdAsync(customerId, connection, cancellationToken);
		var command = new CommandDefinition(sql, new { CustomerId = effectiveCustomerId }, cancellationToken: cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<DashboardSummaryDto>(command);
    }

    public async Task<IReadOnlyList<ActivityDto>> GetInvoiceActivityAsync(Guid customerId, int take, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT TOP (@Take)
            i.IssuedDate AS [Date],
            CONCAT('Invoice ', i.InvoiceNumber, ' generated') AS Title,
            i.TotalAmount AS AmountChange,
            i.Status,
            'Invoice' AS ActivityType
        FROM Invoices i
        WHERE i.CustomerId = @CustomerId
        ORDER BY i.IssuedDate DESC;";

        using var connection = _connectionFactory.CreateConnection();
		var effectiveCustomerId = await ResolveCustomerIdAsync(customerId, connection, cancellationToken);
		var command = new CommandDefinition(sql, new { CustomerId = effectiveCustomerId, Take = take }, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<ActivityDto>(command);
        return results.AsList();
    }

    public async Task<PagedInvoiceListDto> GetInvoicesAsync(Guid customerId, InvoiceFilterDto filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        const string sql = @"DECLARE @Offset INT = (@Page - 1) * @PageSize;

SELECT COUNT(1) AS TotalCount
FROM Invoices i
WHERE i.CustomerId = @CustomerId
    AND (@InvoiceNumber IS NULL OR i.InvoiceNumber LIKE '%' + @InvoiceNumber + '%')
    AND (@Status IS NULL OR i.Status = @Status)
    AND (@MinAmount IS NULL OR i.TotalAmount >= @MinAmount)
    AND (@MaxAmount IS NULL OR i.TotalAmount <= @MaxAmount);

SELECT i.Id, i.InvoiceNumber, i.IssuedDate, i.DueDate, i.Status, i.TotalAmount
FROM Invoices i
WHERE i.CustomerId = @CustomerId
    AND (@InvoiceNumber IS NULL OR i.InvoiceNumber LIKE '%' + @InvoiceNumber + '%')
    AND (@Status IS NULL OR i.Status = @Status)
    AND (@MinAmount IS NULL OR i.TotalAmount >= @MinAmount)
    AND (@MaxAmount IS NULL OR i.TotalAmount <= @MaxAmount)
ORDER BY i.IssuedDate DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        using var connection = _connectionFactory.CreateConnection();
		var effectiveCustomerId = await ResolveCustomerIdAsync(customerId, connection, cancellationToken);
		var parameters = new
        {
			CustomerId = effectiveCustomerId,
            InvoiceNumber = string.IsNullOrWhiteSpace(filter.InvoiceNumber) ? null : filter.InvoiceNumber,
            Status = string.IsNullOrWhiteSpace(filter.Status) ? null : filter.Status,
            MinAmount = filter.MinAmount,
            MaxAmount = filter.MaxAmount,
            Page = page <= 0 ? 1 : page,
            PageSize = pageSize <= 0 ? 10 : pageSize
        };

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        using var multi = await connection.QueryMultipleAsync(command);
        var totalCount = await multi.ReadFirstAsync<int>();
        var items = (await multi.ReadAsync<InvoiceListItemDto>()).AsList();

        return new PagedInvoiceListDto
        {
            Items = items,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyList<InvoiceListItemDto>> GetOpenInvoicesAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT
            Id,
            InvoiceNumber,
            IssuedDate,
            DueDate,
            Status,
            TotalAmount
        FROM Invoices
        WHERE CustomerId = @CustomerId
            AND Status IN ('Pending', 'Overdue')
        ORDER BY DueDate, InvoiceNumber;";

        using var connection = _connectionFactory.CreateConnection();
        var effectiveCustomerId = await ResolveCustomerIdAsync(customerId, connection, cancellationToken);
        var command = new CommandDefinition(sql, new { CustomerId = effectiveCustomerId }, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<InvoiceListItemDto>(command);
        return results.AsList();
    }

    public async Task<IReadOnlyList<InvoiceListItemDto>> GetInvoicesForPeriodAsync(Guid customerId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT
            Id,
            InvoiceNumber,
            IssuedDate,
            DueDate,
            Status,
            TotalAmount
        FROM Invoices
        WHERE CustomerId = @CustomerId
            AND IssuedDate >= @From
            AND IssuedDate <= @To
        ORDER BY IssuedDate;";

        using var connection = _connectionFactory.CreateConnection();
        var effectiveCustomerId = await ResolveCustomerIdAsync(customerId, connection, cancellationToken);
        var parameters = new
        {
            CustomerId = effectiveCustomerId,
            From = from,
            To = to
        };

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        var results = await connection.QueryAsync<InvoiceListItemDto>(command);
        return results.AsList();
    }

    public async Task<InvoiceDetailDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT
            i.Id,
            i.InvoiceNumber,
            i.IssuedDate,
            i.DueDate,
            i.Status,
            c.Name AS BillToName,
            c.Name AS ShipToName,
            (
                c.BillingAddressLine1 + CHAR(10) +
                ISNULL(NULLIF(c.BillingAddressLine2, '' ) + CHAR(10), '') +
                c.BillingCity + ', ' +
                ISNULL(c.BillingState + ' ', '') +
                c.BillingPostalCode + CHAR(10) +
                c.BillingCountry
            ) AS BillToAddress,
            (
                c.ShippingAddressLine1 + CHAR(10) +
                ISNULL(NULLIF(c.ShippingAddressLine2, '' ) + CHAR(10), '') +
                c.ShippingCity + ', ' +
                ISNULL(c.ShippingState + ' ', '') +
                c.ShippingPostalCode + CHAR(10) +
                c.ShippingCountry
            ) AS ShipToAddress,
            i.SubtotalAmount,
            i.DiscountAmount,
            i.TaxAmount,
            i.TotalAmount
        FROM Invoices i
        INNER JOIN Customers c ON c.CustomerId = i.CustomerId
        WHERE i.Id = @InvoiceId;

        SELECT
            Description,
            Quantity,
            UnitPrice,
            LineTotal
        FROM InvoiceItems
        WHERE InvoiceId = @InvoiceId
        ORDER BY Id;";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { InvoiceId = invoiceId }, cancellationToken: cancellationToken);
        using var multi = await connection.QueryMultipleAsync(command);
        var header = await multi.ReadFirstOrDefaultAsync<InvoiceDetailDto>();
        if (header is null)
        {
            return null;
        }

        var lineItems = (await multi.ReadAsync<InvoiceDetailLineItemDto>()).AsList();
        header.LineItems = lineItems;
        return header;
    }

    public async Task MarkInvoicePaidOfflineAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        const string sql = @"UPDATE Invoices SET Status = @Status WHERE Id = @InvoiceId;";

        using var connection = _connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { InvoiceId = invoiceId, Status = "Paid" }, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async Task<Guid> CreateInvoiceAsync(Guid customerId, CreateInvoiceDto invoice, CancellationToken cancellationToken = default)
    {
        const string sql = @"INSERT INTO Invoices (
            Id,
            CustomerId,
            InvoiceNumber,
            IssuedDate,
            DueDate,
            Status,
            SubtotalAmount,
            DiscountAmount,
            TaxAmount,
            TotalAmount
        ) VALUES (
            @Id,
            @CustomerId,
            @InvoiceNumber,
            @IssuedDate,
            @DueDate,
            @Status,
            @SubtotalAmount,
            @DiscountAmount,
            @TaxAmount,
            @TotalAmount
        );";

        var id = Guid.NewGuid();
        using var connection = _connectionFactory.CreateConnection();
        var effectiveCustomerId = await ResolveCustomerIdAsync(customerId, connection, cancellationToken);
        var parameters = new
        {
            Id = id,
            CustomerId = effectiveCustomerId,
            invoice.InvoiceNumber,
            invoice.IssuedDate,
            invoice.DueDate,
            invoice.Status,
            invoice.SubtotalAmount,
            invoice.DiscountAmount,
            invoice.TaxAmount,
            invoice.TotalAmount
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
            throw new InvalidOperationException("No customers are available in the database to associate with invoices.");
        }

        return existingId.Value;
    }
}
