using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CoordinacionOrientadores.Models;
using CoordinacionOrientadores.Services;

namespace CoordinacionOrientadores.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataService _data;

    public HomeController(ILogger<HomeController> logger, DataService data)
    {
        _logger = logger;
        _data = data;
    }

    public IActionResult Index()
    {
        var hoy = DateTime.Today;
        ViewBag.TotalOrientadores = _data.GetOrientadores().Count(o => o.Activo);
        ViewBag.ProcesosHoy = _data.GetAsignaciones().Count(a => a.Fecha.Date == hoy);
        ViewBag.Conflictos = _data.GetAsignaciones().Count(a => a.Estado == EstadoAsignacion.EnConflicto);
        ViewBag.AusentesHoy = _data.GetOrientadores().Count(o => _data.EstaAusente(o.Id, hoy));
        ViewBag.ProximasAsignaciones = _data.GetAsignaciones().Where(a => a.Fecha.Date >= hoy).Take(8).ToList();
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
