using server.Entities;

namespace server.Services;

public interface ICompaniesService
{
    Task<List<Company>> GetCompanies();
}