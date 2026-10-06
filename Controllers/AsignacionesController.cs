using CoordinacionOrientadores.Models;
using CoordinacionOrientadores.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoordinacionOrientadores.Controllers
{
    public class AsignacionesController : Controller
    {
        private readonly DataService _data;
        public AsignacionesController(DataService data) => _data = data;

        // Lista de todas las asignaciones, con filtros opcionales
        public IActionResult Index(int? orientadorId, DateTime? desde, DateTime? hasta)
        {
            var asignaciones = _data.GetAsignaciones();

            if (orientadorId.HasValue)
                asignaciones = asignaciones.Where(a => a.OrientadorId == orientadorId).ToList();
            if (desde.HasValue)
                asignaciones = asignaciones.Where(a => a.Fecha.Date >= desde.Value.Date).ToList();
            if (hasta.HasValue)
                asignaciones = asignaciones.Where(a => a.Fecha.Date <= hasta.Value.Date).ToList();

            ViewBag.Orientadores = _data.GetOrientadores();
            ViewBag.OrientadorId = orientadorId;
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            return View(asignaciones);
        }

        // Paso 1: elegir qué proceso/horario se quiere cubrir
        public IActionResult Nueva()
        {
            ViewBag.Procesos = _data.GetProcesos();
            return View(new NuevaAsignacionInput { HoraInicio = new TimeSpan(9, 0, 0), HoraFin = new TimeSpan(10, 0, 0) });
        }

        // Paso 2: el sistema reparte la carga y sugiere a quién asignarle, de forma equitativa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Sugerencias(NuevaAsignacionInput datos)
        {
            var proceso = _data.GetProceso(datos.ProcesoId);
            if (proceso == null)
            {
                ModelState.AddModelError("", "Seleccione un proceso válido.");
                ViewBag.Procesos = _data.GetProcesos();
                return View("Nueva", datos);
            }

            var sugerencias = _data.SugerirOrientadores(datos.Fecha, datos.HoraInicio, datos.HoraFin)
                .Select(s => new SugerenciaOrientadorVM
                {
                    OrientadorId = s.Orientador.Id,
                    Nombre = s.Orientador.Nombre,
                    Sede = s.Orientador.Sede,
                    HorasAsignadasSemana = Math.Round(s.HorasAsignadasSemana, 1),
                    HorasContrato = s.Orientador.HorasSemanalesContrato,
                    Disponible = s.Disponible,
                    Motivo = s.Motivo
                }).ToList();

            var vm = new SugerenciasVM
            {
                Datos = datos,
                ProcesoNombre = proceso.Nombre,
                Sugerencias = sugerencias
            };
            return View(vm);
        }

        // Paso 3: confirmar la asignación con el orientador elegido (de la lista sugerida)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmar(int orientadorId, int procesoId, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin, string sede, string? enlaceReunion, string? notas)
        {
            var asignacion = new Asignacion
            {
                OrientadorId = orientadorId,
                ProcesoId = procesoId,
                Fecha = fecha,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                Sede = sede,
                EnlaceReunion = enlaceReunion,
                Notas = notas,
                Estado = EstadoAsignacion.Programado
            };
            _data.SaveAsignacion(asignacion);
            TempData["Mensaje"] = "Asignación creada y notificada en el calendario del orientador.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var a = _data.GetAsignacion(id);
            if (a == null) return NotFound();
            ViewBag.Procesos = _data.GetProcesos();
            ViewBag.OrientadoresList = _data.GetOrientadores();
            return View(a);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Asignacion model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Procesos = _data.GetProcesos();
                ViewBag.OrientadoresList = _data.GetOrientadores();
                return View(model);
            }
            _data.SaveAsignacion(model);
            TempData["Mensaje"] = "Asignación actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _data.DeleteAsignacion(id);
            TempData["Mensaje"] = "Asignación eliminada.";
            return RedirectToAction(nameof(Index));
        }

        // Para cuando quedó "En conflicto" por una ausencia: reasignar rápidamente a un sustituto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Sustituir(int id, int nuevoOrientadorId)
        {
            var a = _data.GetAsignacion(id);
            if (a == null) return NotFound();

            a.OrientadorOriginalId = a.OrientadorOriginalId ?? a.OrientadorId;
            a.OrientadorId = nuevoOrientadorId;
            a.Estado = EstadoAsignacion.Sustituido;
            _data.SaveAsignacion(a);

            TempData["Mensaje"] = "Se asignó un sustituto para esta sesión.";
            return RedirectToAction(nameof(Index));
        }
    }
}
