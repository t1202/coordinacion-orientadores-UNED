using CoordinacionOrientadores.Models;
using CoordinacionOrientadores.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoordinacionOrientadores.Controllers
{
    public class ProcesosController : Controller
    {
        private readonly DataService _data;
        public ProcesosController(DataService data) => _data = data;

        public IActionResult Index() => View(_data.GetProcesos());

        public IActionResult Create() => View(new Proceso());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Proceso model)
        {
            if (!ModelState.IsValid) return View(model);
            _data.SaveProceso(model);
            TempData["Mensaje"] = $"Proceso \"{model.Nombre}\" guardado.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var p = _data.GetProceso(id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Proceso model)
        {
            if (!ModelState.IsValid) return View(model);
            _data.SaveProceso(model);
            TempData["Mensaje"] = $"Proceso \"{model.Nombre}\" actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var p = _data.GetProceso(id);
            _data.DeleteProceso(id);
            TempData["Mensaje"] = $"Proceso \"{p?.Nombre}\" eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
