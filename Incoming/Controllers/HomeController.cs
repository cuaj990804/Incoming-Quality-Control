using DocumentFormat.OpenXml.InkML;
using Incoming.Data;
using Incoming.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Incoming.Controllers
{
    [Authorize(Roles = "ADMINISTRATOR,SUPERVISOR,USER")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IncomingContext _context;

        public HomeController(ILogger<HomeController> logger, IncomingContext context)
        {
            _logger = logger;
            _context = context;
        }


        public IActionResult Index()
        {
            ViewBag.ProgramList = _context.InspectionPrograms.Select(p => p.ProgramName).ToList();
            ViewBag.ProgramListIncoming = _context.InspectionPrograms
            .Select(p => p.ProgramName)
            .Where(name => name != "EJ")
            .ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
