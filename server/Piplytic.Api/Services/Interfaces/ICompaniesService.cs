using PipLytic.Api.Entities;

namespace PipLytic.Api.Services;

public interface ICompaniesService
{
    Task<List<Company>> GetCompanies();
}