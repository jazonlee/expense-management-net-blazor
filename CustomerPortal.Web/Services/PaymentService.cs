using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerPortal.Core.Application.DTOs;
using CustomerPortal.Core.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CustomerPortal.Web.Services;

public class PaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<PaymentDto>> GetPaymentsAsync(Guid customerId, DateTime? from, DateTime? to, string? method, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.Payments.GetPaymentsAsync(customerId, from, to, method, cancellationToken);
    }

    public async Task<Guid> CreatePaymentAsync(Guid customerId, CreatePaymentDto payment, CancellationToken cancellationToken = default)
    {
        var id = await _unitOfWork.Payments.CreatePaymentAsync(customerId, payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task<StatementOfAccountDto> GetCurrentStatementAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var start = new DateTime(today.Year, today.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var entries = await BuildStatementEntriesAsync(customerId, start, end, cancellationToken);
        var closing = entries.Count == 0 ? 0m : entries[^1].Balance;

        return new StatementOfAccountDto
        {
            PeriodStart = start,
            PeriodEnd = end,
            OpeningBalance = 0m,
            ClosingBalance = closing
        };
    }

    public async Task<byte[]> GenerateStatementPdfAsync(Guid customerId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var entries = await BuildStatementEntriesAsync(customerId, from, to, cancellationToken);
        var closing = entries.Count == 0 ? 0m : entries[^1].Balance;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Statement of Account").FontSize(20).Bold();
                        col.Item().Text($"Period: {from:MMM dd, yyyy} - {to:MMM dd, yyyy}").FontSize(11);
                    });
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                                .PaddingVertical(5)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Text("Date");

                            header.Cell()
                                .PaddingVertical(5)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Text("Description");

                            header.Cell()
                                .PaddingVertical(5)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .AlignRight()
                                .Text("Debit");

                            header.Cell()
                                .PaddingVertical(5)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .AlignRight()
                                .Text("Credit");

                            header.Cell()
                                .PaddingVertical(5)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .AlignRight()
                                .Text("Balance");
                        });

                        foreach (var entry in entries)
                        {
                            table.Cell()
                                .PaddingVertical(4)
                                .BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten4)
                                .Text(entry.Date.ToString("yyyy-MM-dd"));

                            table.Cell()
                                .PaddingVertical(4)
                                .BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten4)
                                .Text(entry.Description);

                            table.Cell()
                                .PaddingVertical(4)
                                .BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten4)
                                .AlignRight()
                                .Text(entry.Debit == 0m ? "" : entry.Debit.ToString("C"));

                            table.Cell()
                                .PaddingVertical(4)
                                .BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten4)
                                .AlignRight()
                                .Text(entry.Credit == 0m ? "" : entry.Credit.ToString("C"));

                            table.Cell()
                                .PaddingVertical(4)
                                .BorderBottom(0.5f)
                                .BorderColor(Colors.Grey.Lighten4)
                                .AlignRight()
                                .Text(entry.Balance.ToString("C"));
                        }
                    });

                    col.Item().AlignRight().Text(text =>
                    {
                        text.Span("Closing Balance: ").SemiBold();
                        text.Span(closing.ToString("C"));
                    });
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        });

        return document.GeneratePdf();
    }

    private async Task<List<StatementEntry>> BuildStatementEntriesAsync(Guid customerId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var invoices = await _unitOfWork.Invoices.GetInvoicesForPeriodAsync(customerId, from, to, cancellationToken);
        var payments = await _unitOfWork.Payments.GetPaymentsAsync(customerId, from, to, null, cancellationToken);

        var entries = new List<StatementEntry>();

        entries.AddRange(invoices.Select(i => new StatementEntry
        {
            Date = i.IssuedDate,
            Description = $"Invoice {i.InvoiceNumber}",
            Debit = i.TotalAmount,
            Credit = 0m
        }));

        entries.AddRange(payments.Select(p => new StatementEntry
        {
            Date = p.Date,
            Description = $"Payment {p.PaymentRef} ({p.Method})",
            Debit = 0m,
            Credit = p.Amount
        }));

        entries = entries
            .OrderBy(e => e.Date)
            .ThenBy(e => e.Description)
            .ToList();

        decimal balance = 0m;
        foreach (var entry in entries)
        {
            balance += entry.Debit - entry.Credit;
            entry.Balance = balance;
        }

        return entries;
    }

    private sealed class StatementEntry
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }
}
