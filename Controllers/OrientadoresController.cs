using CoordinacionOrientadores.Models;
using CoordinacionOrientadores.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoordinacionOrientadores.Controllers
{
    public class OrientadoresController : Controller
    {
        private readonly DataService _data;
        public OrientadoresController(DataService data) => _data = data;

        public IActionResult Index() => View(_data.GetOrientadores());

        public IActionResult Create() => View(new Orientador());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Orientador model)
        {
            if (!ModelState.IsValid) return View(model);
            _data.SaveOrientador(model);
            TempData["Mensaje"] = $"Orientador \"{model.Nombre}\" guardado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var o = _data.GetOrientador(id);
            if (o == null) return NotFound();
            return View(o);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Orientador model)
        {
            if (!ModelState.IsValid) return View(model);
            _data.SaveOrientador(model);
            TempData["Mensaje"] = $"Orientador \"{model.Nombre}\" actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var o = _data.GetOrientador(id);
            _data.DeleteOrientador(id);
            TempData["Mensaje"] = $"Orientador \"{o?.Nombre}\" eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
