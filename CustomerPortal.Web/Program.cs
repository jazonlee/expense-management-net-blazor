using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using CustomerPortal.Web.Data;
using CustomerPortal.Core.Application.Interfaces;
using CustomerPortal.Web.Infrastructure.Persistence;
using CustomerPortal.Web.Services;
using System.Text;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Infrastructure & application services
builder.Services.AddScoped<SqlConnectionFactory>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<PaymentService>();

var app = builder.Build();

QuestPDF.Settings.License = LicenseType.Community;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGet("/soa/pdf", async (DateTime from, DateTime to, PaymentService paymentService, HttpContext httpContext) =>
{
    var customerId = Guid.Empty; // TODO: replace with real current customer resolution
    var bytes = await paymentService.GenerateStatementPdfAsync(customerId, from, to, httpContext.RequestAborted);
    return Results.File(bytes, "application/pdf", "statement-of-account.pdf");
});

app.Run();
