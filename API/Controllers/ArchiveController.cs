using Microsoft.AspNetCore.Mvc;
using RSSI_Nuro.Authorization;
using RSSI_Nuro.Data;
using Microsoft.EntityFrameworkCore;

namespace RSSI_Nuro.Controllers;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(AuthFilter))]

public class ArchiveController : ControllerBase
{
    private readonly ApplicationDbContext _dbcontext;

    public ArchiveController(ApplicationDbContext context)
    {
        _dbcontext = context;
    }

    [HttpGet("get")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Get()
    {
        try
        {

            // Get the current date and subtract 5 months to filter the data
            var fiveMonthsAgo = DateTime.UtcNow.AddMonths(-5);

            // Retrieve data grouped by month and calculate averages
            var monthlyAverages = await _dbcontext.ArchiveRecords
                .Where(record => record.Timestamp >= fiveMonthsAgo)
                .GroupBy(record => new { record.Timestamp.Year, record.Timestamp.Month })
                .Select(group => new
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Bt = group.Average(record => record.Bt),
                    BxGSM = group.Average(record => record.BxGSM),
                    ByGSM = group.Average(record => record.ByGSM),
                    BzGSM = group.Average(record => record.BzGSM),
                    Intensity = group.Average(record => record.Intensity),
                    Inclination = group.Average(record => record.Inclination),
                    Declination = group.Average(record => record.Declination),
                })
                .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
                .ToListAsync();

            if (monthlyAverages == null || !monthlyAverages.Any())
            {
                return NoContent();
            }

            // Return the data in the specified structure
            return Ok(new
            {
                bt = monthlyAverages.Select(m => m.Bt).ToList(),
                bx_gsm = monthlyAverages.Select(m => m.BxGSM).ToList(),
                by_gsm = monthlyAverages.Select(m => m.ByGSM).ToList(),
                bz_gsm = monthlyAverages.Select(m => m.BzGSM).ToList(),
                intensity = monthlyAverages.Select(m => m.Intensity).ToList(),
                inclination = monthlyAverages.Select(m => m.Inclination).ToList(),
                declination = monthlyAverages.Select(m => m.Declination).ToList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

}
