using Microsoft.EntityFrameworkCore;
using PipLytic.Api.Data;
using PipLytic.Api.Entities;
using PipLytic.Api.Exceptions;

namespace PipLytic.Api.Services;

public class CompaniesService(AppDbContext db) : ICompaniesService
{
    public async Task<List<Company>> GetCompanies() => await db.Companies.ToListAsync();
}