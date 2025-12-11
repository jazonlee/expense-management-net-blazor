using CustomerPortal.Web.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CustomerPortal.Tests.Repositories;

public class InvoiceRepositoryTests
{
    private readonly SqlConnectionFactory _connectionFactory;

    public InvoiceRepositoryTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"ConnectionStrings:CustomerPortalDb", "Server=.\\SQLEXPRESS;Database=CustomerPortalDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _connectionFactory = new SqlConnectionFactory(configuration);
    }

    [Fact(Skip = "Placeholder fixture; enable after schema and test data are applied.")]
    public void Can_Create_InvoiceRepository()
    {
        var repository = new InvoiceRepository(_connectionFactory);
        Assert.NotNull(repository);
    }
}
