using CoordinacionOrientadores.Models;
using CoordinacionOrientadores.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoordinacionOrientadores.Controllers
{
    public class AusenciasController : Controller
    {
        private readonly DataService _data;
        public AusenciasController(DataService data) => _data = data;

        public IActionResult Index()
        {
            var orientadores = _data.GetOrientadores().ToDictionary(o => o.Id);
            ViewBag.Orientadores = orientadores;
            return View(_data.GetAusencias());
        }

        public IActionResult Create()
        {
            ViewBag.Orientadores = _data.GetOrientadores();
            return View(new Ausencia());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Ausencia model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Orientadores = _data.GetOrientadores();
                return View(model);
            }

            var conflictos = _data.SaveAusenciaYDetectarConflictos(model);

            if (conflictos.Count > 0)
            {
                TempData["Mensaje"] = $"Ausencia registrada. ⚠ Se encontraron {conflictos.Count} proceso(s) ya asignados en ese rango de fechas — quedaron marcados como \"En conflicto\" en Asignaciones para que les busque sustituto.";
            }
            else
            {
                TempData["Mensaje"] = "Ausencia registrada. No tenía procesos asignados en ese rango.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _data.DeleteAusencia(id);
            TempData["Mensaje"] = "Ausencia eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
