using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PipLytic.Api.Data;
using PipLytic.Api.Entities;
using PipLytic.Api.Services;

namespace PipLytic.Tests.Services;

public class CompaniesServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly CompaniesService _sut;

    public CompaniesServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _sut = new CompaniesService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetCompanies_WhenNoCompaniesExist_ReturnsEmptyList()
    {
        var result = await _sut.GetCompanies();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCompanies_WhenCompaniesExist_ReturnsAll()
    {
        _db.Companies.AddRange(
            new Company { Id = Guid.NewGuid(), Symbol = "AAPL", Name = "Apple Inc." },
            new Company { Id = Guid.NewGuid(), Symbol = "NVDA", Name = "NVIDIA Corporation" }
        );
        await _db.SaveChangesAsync();

        var result = await _sut.GetCompanies();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCompanies_ReturnsCorrectSymbolsAndNames()
    {
        _db.Companies.Add(new Company { Id = Guid.NewGuid(), Symbol = "MSFT", Name = "Microsoft Corporation" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetCompanies();

        result.Should().ContainSingle(c => c.Symbol == "MSFT" && c.Name == "Microsoft Corporation");
    }
}
