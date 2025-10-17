using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Entities;
using server.Exceptions;

namespace server.Services;

public class CompaniesService(AppDbContext db) : ICompaniesService
{
    public async Task<List<Company>> GetCompanies() => await db.Companies.ToListAsync();
}