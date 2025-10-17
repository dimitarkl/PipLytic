using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Entities;
using server.Exceptions;

namespace server.Services;

public class CompaniesService(AppDbContext db) : ICompaniesService
{

    public async Task<List<Company>> GetCompanies()
    {
        var companies = await db.Companies.ToListAsync();
        if (companies.Count == 0)
            throw new NotFoundException("No companies found");
        return companies;
    }
}