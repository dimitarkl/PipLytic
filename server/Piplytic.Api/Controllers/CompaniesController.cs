using Microsoft.AspNetCore.Mvc;
using PipLytic.Api.Entities;
using PipLytic.Api.Services;

namespace PipLytic.Api.Controllers;

[Route("companies")]
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