using Microsoft.AspNetCore.Mvc;
using server.Entities;
using server.Services;

namespace server.Controllers;

[Route("/api/companies")]
[ApiController]
public class CompaniesController(ICompaniesService companyService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Company>>> GetCompanies()
    {
        var companies = await companyService.GetCompanies();
        return Ok(companies);
    }
}