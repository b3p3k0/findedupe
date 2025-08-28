namespace Jellyfin.Plugin.FinDeDupe.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>Minimal API surface for FinDeDupe.</summary>
    [ApiController]
    [Route("Plugins/FinDeDupe")]
    public class FinDeDupeController : ControllerBase
    {
        /// <summary>Simple health endpoint to validate plugin wiring.</summary>
        /// <returns>An <see cref="IActionResult"/> indicating success.</returns>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return this.Ok(new { plugin = "FinDeDupe", status = "ok" });
        }
    }
}
