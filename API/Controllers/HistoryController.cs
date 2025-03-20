using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSSI_Nuro.Authorization;
using RSSI_Nuro.Data;

namespace RSSI_webAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(AuthFilter))]
public class HistoryController : ControllerBase
{

private readonly ApplicationDbContext _dbcontext;

public HistoryController(ApplicationDbContext context)
{
    _dbcontext = context;
}

[HttpGet("day")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<int> ReconnectionOccurenceOverDay(){
    
    var today = DateTime.Today;

        var count = await _dbcontext.ReconnectionRecords
                    .Where(r => r.Timestamp >= today && r.Timestamp < today.AddDays(1))
                    .CountAsync();

        return count;
}

[HttpGet("week")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<int> ReconnectionOccurenceOverWeek(){
        var today = DateTime.Today;
        var lastWeek = today.AddDays(-7);

        var count = await _dbcontext.ReconnectionRecords
                    .Where(r => r.Timestamp >= lastWeek && r.Timestamp < today.AddDays(1))
                    .CountAsync();

        return count;
}

[HttpGet("month")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<int> ReconnectionOccurenceOverMonth(){
        var today = DateTime.Today;
        var lastMonth = today.AddDays(-30);

        var count = await _dbcontext.ReconnectionRecords
                    .Where(r => r.Timestamp >= lastMonth && r.Timestamp < today.AddDays(1))
                    .CountAsync();

        return count;
}

[HttpGet("summary")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<int[]> ReconnectionOcurenceSummary(){
    var summary = new int[3];
    summary[0] = await ReconnectionOccurenceOverDay();
    summary[1] = await ReconnectionOccurenceOverWeek();
    summary[2] = await ReconnectionOccurenceOverMonth();
    return summary;
}

}
