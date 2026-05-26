using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Incoming.Controllers
{
    public class ShutdownController : Controller
    {
        [HttpPost]
        public IActionResult Shutdown()
        {
            // Ejecutar el comando para apagar la PC
            Process.Start("shutdown", "/s /f /t 0"); // /s: apaga el sistema, /f: fuerza el cierre, /t 0: tiempo de 0 segundos
            return Ok();
        }
       
    }
}
