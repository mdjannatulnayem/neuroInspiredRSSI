using Microsoft.AspNetCore.Mvc;
using RSSI_Nuro.Authorization;
using RSSI_Nuro.Data;
using RSSI_Nuro.Models;

namespace RSSI_webAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[ServiceFilter(typeof(AuthFilter))]

public class FeedbackController : ControllerBase
{
    private readonly ApplicationDbContext _dbcontext;

    public FeedbackController(ApplicationDbContext context)
    {
        _dbcontext = context;
    }


    [HttpPost("feedback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Feedback(ReconDataModel feedback)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid model state!");
            feedback.Timestamp = DateTime.UtcNow;
            await _dbcontext.ReconnectionRecords.AddAsync(feedback);
            await _dbcontext.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500,ex.Message);
        }
    }


    [HttpPost("archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Archive(ArchiveDataModel archive)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid model state!");
            archive.Timestamp = DateTime.UtcNow;
            await _dbcontext.ArchiveRecords.AddAsync(archive);
            await _dbcontext.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}