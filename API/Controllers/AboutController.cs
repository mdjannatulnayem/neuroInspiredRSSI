using Microsoft.AspNetCore.Mvc;

namespace RSSI_Nuro.Controllers;

[Route("")]
public class AboutController : Controller
{
    [HttpGet("apidef")]
    public ActionResult ApiDef()
    {
        return View();
    }
}
