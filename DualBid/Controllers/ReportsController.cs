using DualBid.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DualBid.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IComicReporteService _reportService;
    private readonly IServiceCategory _serviceCategory;

    public ReportsController(IComicReporteService reportService, IServiceCategory serviceCategory)
    {
        _reportService = reportService;
        _serviceCategory = serviceCategory;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _serviceCategory.ListAsync();
        return Ok(categories);
    }


    [HttpGet("category-history")]
    public async Task<IActionResult> CategoryHistory(
    [FromQuery] int? categoryId,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to)
    {
        // Al menos categoría O fechas completas
        if (!categoryId.HasValue && (!from.HasValue || !to.HasValue))
            return BadRequest("Debes seleccionar una categoría o un rango de fechas completo.");

        if (from.HasValue && to.HasValue && from > to)
            return BadRequest("La fecha de inicio no puede ser mayor a la fecha de fin.");

        var pdf = await _reportService.GenerateReportCategoryHistoryAsync(categoryId, from, to);
        Response.Headers.Add("Content-Disposition", "inline; filename=reporte.pdf");
        return File(pdf, "application/pdf");
    }
}